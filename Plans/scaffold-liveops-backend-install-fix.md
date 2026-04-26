# Scaffold LiveOps — Backend~ install fix

Handoff notes for fixing the `com.scaffold.liveops` package so a fresh install of `Backend~` builds without manual edits to the consumer repo.

## Symptoms in the consumer repo (before fix)

After installing the package and running the LiveOps Cloud Code deploy build, two errors occurred in sequence:

1. `MSB4019` — imported project `...\Deploy\LiveOps\Deploy\Build\Scaffold.LiveOps.Deploy.targets` was not found (the path is wrong: `Deploy\` appears twice).
2. After fixing #1, `CS0103: The name 'LiveOpsManifest' does not exist in the current context` in `Deploy/LiveOps/Initialize/ModuleConfig.cs`.

Both originate in the package template, not in consumer code.

## Root causes

### 1. `$(LiveOpsRoot)` and `$(RepositoryRoot)` are referenced but never defined

`Backend~/Deploy/LiveOps/LiveOps.csproj` imports:

```xml
<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Deploy.targets" />
<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.TemplateSync.targets" />
```

And `Backend~/Deploy/Build/Scaffold.LiveOps.Common.props` uses both:

```xml
<_ScaffoldGenProj>$(RepositoryRoot)Generators\Scaffold.LiveOps.Bootstrap.Generators\Scaffold.LiveOps.Bootstrap.Generators.csproj</_ScaffoldGenProj>
<_ScaffoldGenDll>$(LiveOpsRoot)Deploy\Tools\Generators\Scaffold.LiveOps.Bootstrap.Generators.dll</_ScaffoldGenDll>
```

Neither `Microsoft.NET.Sdk` nor anything else in the install defines them. They were almost certainly inherited from a `Directory.Build.props` in the scaffold development repo, which is not copied into consumer repos.

Result: `$(LiveOpsRoot)` expands to empty, the import path becomes relative, points to a non-existent location, and `MSB4019` fires.

### 2. `Scaffold.LiveOps.Common.props` is never imported by the host

`LiveOps.csproj` only imports `Scaffold.LiveOps.Deploy.targets` (project-reference globbing) and `Scaffold.LiveOps.TemplateSync.targets` (post-build sync). It does **not** import `Common.props`, which is what wires the source generator (`Scaffold.LiveOps.Bootstrap.Generators`) responsible for emitting `LiveOpsManifest`.

Without it, `ModuleConfig.cs` references `LiveOpsManifest.Entries`, the type is never generated, and `CS0103` fires.

`LiveOps.Core.csproj` and `LiveOps.DTO.csproj` also do not import `Common.props`, so they never receive the `[AssemblyMetadata("ScaffoldLiveOpsAssembly", "true")]` marker that the generator scans for. Verify on the scaffold side whether the generator needs that attribute on Core/DTO to discover types — if yes, those projects must also import `Common.props`.

## Recommended fix — ship `Directory.Build.props` in `Backend~/Deploy/`

Single source of truth, zero per-project boilerplate, robust against future csprojs being added:

```xml
<!-- Backend~/Deploy/Directory.Build.props -->
<Project>
  <PropertyGroup>
    <!-- LiveOpsRoot = the LiveOps/ directory in the consumer repo (parent of Deploy/, Scaffold/, Game/). -->
    <LiveOpsRoot Condition="'$(LiveOpsRoot)' == ''">$([MSBuild]::NormalizeDirectory('$(MSBuildThisFileDirectory)..'))</LiveOpsRoot>
    <!-- RepositoryRoot = consumer repo root (parent of LiveOps/). Override in consumer Directory.Build.user.props if the layout differs. -->
    <RepositoryRoot Condition="'$(RepositoryRoot)' == ''">$([MSBuild]::NormalizeDirectory('$(LiveOpsRoot)..'))</RepositoryRoot>
  </PropertyGroup>

  <Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Common.props"
          Condition="Exists('$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Common.props')" />
</Project>
```

With this in place:

- `LiveOps.csproj`, `LiveOps.Core.csproj`, and `LiveOps.DTO.csproj` all inherit `LiveOpsRoot` / `RepositoryRoot` automatically.
- All three pick up `Common.props`, so:
  - the generator analyzer is added (from the `Generators/` source if present in the repo, else from the prebuilt DLL at `LiveOps/Deploy/Tools/Generators/Scaffold.LiveOps.Bootstrap.Generators.dll`),
  - and each assembly is marked with `ScaffoldLiveOpsAssembly=true` so the generator can discover types in it.
- The existing `<Import>` lines in `LiveOps.csproj` continue to work unchanged.

## Minimum-change alternative (per-project)

If a `Directory.Build.props` is undesirable, inline the fix in `Backend~/Deploy/LiveOps/LiveOps.csproj` only:

```xml
<PropertyGroup>
  <LiveOpsRoot Condition="'$(LiveOpsRoot)' == ''">$(MSBuildThisFileDirectory)..\..\</LiveOpsRoot>
  <RepositoryRoot Condition="'$(RepositoryRoot)' == ''">$(MSBuildThisFileDirectory)..\..\..\</RepositoryRoot>
</PropertyGroup>

...

<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Common.props" />
<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Deploy.targets" />
<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.TemplateSync.targets" />
```

Caveats:

- Assumes a fixed install depth (`<repo>/LiveOps/Deploy/LiveOps/`). Breaks if the install root changes.
- Leaves Core / DTO without the assembly metadata attribute (may or may not matter — see open questions).

## Open questions to confirm on the scaffold side

1. Does `Scaffold.LiveOps.Bootstrap.Generators` require the `[AssemblyMetadata("ScaffoldLiveOpsAssembly","true")]` attribute on referenced assemblies (Core / DTO / feature modules) to discover types? If yes, those csprojs must also import `Common.props`, which strongly favors the `Directory.Build.props` approach.
2. Is `RepositoryRoot` used anywhere outside the optional dev-only generator-source path lookup in `Common.props`? If it is only there as a "build from generator source if available" optimization, consumer installs don't strictly need it — they fall back to the bundled DLL. In that case the only mandatory consumer property is `LiveOpsRoot`.
3. The package install pipeline (whatever copies `Backend~/` contents into `<repo>/LiveOps/`) must also copy `Directory.Build.props` and refresh it on every package update.

## Reference: what was changed locally as a workaround

`<repo>/LiveOps/Deploy/LiveOps/LiveOps.csproj`:

- Added `<LiveOpsRoot>` and `<RepositoryRoot>` properties.
- Added `<Import Project="$(LiveOpsRoot)Deploy\Build\Scaffold.LiveOps.Common.props" />` before the existing imports.

These are local-only patches that will be overwritten on the next package install — the real fix belongs in the scaffold package as described above.