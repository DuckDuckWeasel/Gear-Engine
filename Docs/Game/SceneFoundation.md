# Scene Foundation

## TL;DR

- **`Game.SceneFoundation`** is a small runtime assembly that owns shared scene DI: cross-layer resolver, Addressables, navigation, and events.
- Feature scenes inherit **`SceneFoundationScope`** and only implement **`InstallFeatureServices`** (feature installers + bootstrap registration).
- New systems add **`Game.<Feature>`** with a scope, optional installer, bootstrap, and EditMode tests under **`Tests/Editor/`**.
- Full module code: [`Assets/GearEngine/Scripts/Game/SceneFoundation/`](../../Assets/GearEngine/Scripts/Game/SceneFoundation/).

## Responsibilities

**Owns**

- `SceneFoundationScope` base class for scene-level `LifetimeScope`.
- Validation of **`NavigationSettings`** and **`navigationViewHolder`** on every scene scope.
- Registration of **`CrossLayerObjectResolver`** / **`ICrossLayerObjectResolver`** and the post-build scope hook.
- Installation of **`AddressablesInstaller`**, **`NavigationInstaller`**, and **`EventsInstaller`** in a consistent order.

**Does not own**

- Feature domain services, view models, or feature-specific installers.
- Scene bootstrap types or `RegisterComponent` for bootstraps (derived scopes own that).
- UI layout, prefabs, or editor menu tools (except documentation pointers).

**Boundaries**

- Runtime assembly: references **`Scaffold.*`**, **`VContainer`**, **`UnityEngine`** only (no references to **`Game.GearEngine`**, **`Game.CarSimulation`**, **`Game.Race`**).

## Public API

| Symbol | Purpose | Inputs | Outputs | Failure / edge behavior |
|--------|---------|--------|---------|-------------------------|
| `SceneFoundationScope` | Abstract scene `LifetimeScope` with shared infra | Serialized `navigationSettings`, `navigationViewHolder`; derived `InstallFeatureServices` | Configured VContainer builder | Throws **`InvalidOperationException`** if navigation fields are null; logs cross-layer failures with **`Debug.LogError`** |

## Setup / Integration

1. Add **`Game.SceneFoundation`** to your feature **`.asmdef`** `references` if your scope or editor code touches **`SceneFoundationScope`** (editor assemblies that use scopes must reference it too).
2. Change your feature scope to **`class MyScope : SceneFoundationScope`**.
3. Implement **`ValidateSceneAssignments()`** for feature serialized fields (configs, bootstrap, etc.).
4. Implement **`InstallFeatureServices(IContainerBuilder)`**:
   - run feature **`*Installer`** types
   - **`builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf()`**
   - register optional scene-only instances (e.g. feature toggles).
5. In the scene, assign **`Navigation Settings`** and a **`navigationViewHolder`** transform that parents the navigation context view (same as before).

**Common mistakes**

- Forgetting **`using VContainer.Unity`** in a derived scope when calling **`RegisterComponent`**.
- Editor assembly references only **`Game.<Feature>`** but not **`Game.SceneFoundation`** after inheriting from **`SceneFoundationScope`** in runtime — add **`Game.SceneFoundation`** to the **Editor** `.asmdef` too.

## How to Use

1. Create **`Game.<Feature>.asmdef`** under **`Assets/GearEngine/Scripts/Game/<Feature>/`** with references your feature needs plus **`Game.SceneFoundation`**.
2. Add **`Bootstrap/<Feature>Scope.cs`** inheriting **`SceneFoundationScope`**.
3. Add **`Bootstrap/<Feature>Installer.cs`** if you have multiple registrations to keep **`Configure`** small.
4. Add **`Bootstrap/<Feature>Bootstrap.cs`** as **`MonoBehaviour, IInitializable`** that calls **`INavigation.Open(...)`** with your view model.
5. Add **`Tests/Editor/Game.<Feature>.Tests.asmdef`** and tests for installers / bootstrap behavior.
6. Run **`.agents/scripts/validate-changes.cmd`** from the repository root.

## Examples

### Minimal integration (existing pattern)

See:

- [`RaceScope`](../../Assets/GearEngine/Scripts/Game/Race/Bootstrap/RaceScope.cs) — composes gear + car installers, registers **`RaceBootstrap`**.
- [`CarTrackScope`](../../Assets/GearEngine/Scripts/Game/CarSimulation/Bootstrap/CarTrackScope.cs) — car track installer + **`CarTrackBootstrap`**.
- [`GearMechanicsScope`](../../Assets/GearEngine/Scripts/Game/GearEngine/Bootstrap/GearMechanicsScope.cs) — gear installer + optional feature toggle + **`GearTestSceneBootstrap`**.

### Sample: `TestSystem` (new feature)

**Layout**

- `Assets/GearEngine/Scripts/Game/TestSystem/Game.TestSystem.asmdef`
- `Assets/GearEngine/Scripts/Game/TestSystem/Bootstrap/TestSystemScope.cs`
- `Assets/GearEngine/Scripts/Game/TestSystem/Bootstrap/TestSystemInstaller.cs`
- `Assets/GearEngine/Scripts/Game/TestSystem/Bootstrap/TestSystemBootstrap.cs`
- `Assets/GearEngine/Scripts/Game/TestSystem/TestSystemViewModel.cs`
- `Assets/GearEngine/Scripts/Game/TestSystem/Presentation/TestSystemView.cs` (if needed)
- `Assets/GearEngine/Scripts/Game/TestSystem/Tests/Editor/Game.TestSystem.Tests.asmdef`

**`TestSystemScope`**

```csharp
public sealed class TestSystemScope : SceneFoundationScope
{
    [SerializeField] private TestSystemConfigSO config;
    [SerializeField] private TestSystemBootstrap sceneBootstrap;

    protected override void ValidateSceneAssignments()
    {
        if (config == null)
        {
            throw new InvalidOperationException("[TestSystemScope] Assign config.");
        }

        if (sceneBootstrap == null)
        {
            throw new InvalidOperationException("[TestSystemScope] Assign sceneBootstrap.");
        }
    }

    protected override void InstallFeatureServices(IContainerBuilder builder)
    {
        new TestSystemInstaller(config).Install(builder);
        builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
    }
}
```

**`TestSystemBootstrap`**

```csharp
public sealed class TestSystemBootstrap : MonoBehaviour, IInitializable
{
    [SerializeField] private TestSystemStartData startData;

    [Inject] private INavigation navigation;

    public void Initialize()
    {
        try
        {
            if (startData == null)
            {
                throw new InvalidOperationException("[TestSystemBootstrap] Start data is missing.");
            }

            navigation.Open(new TestSystemViewModel(startData));
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TestSystemBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}
```

### Error / guard example

If **`navigationSettings`** is not assigned, **`SceneFoundationScope`** throws before feature installers run:

```text
InvalidOperationException: [MyFeatureScope] Assign navigationSettings (e.g. Assets/Navigation/Navigation Settings.asset).
```

## Best Practices

- Keep **`TestSystemInstaller`** (or any feature installer) free of Addressables, navigation, events, and cross-layer wiring — the foundation already installs those.
- Always assign **`navigationViewHolder`** to the transform that parents the view Scaffold navigation will bind.
- Use **`ValidateSceneAssignments`** for feature null checks; let the base class own navigation validation.
- Prefer one scope per scene root; avoid multiple competing **`LifetimeScope`** roots unless you intentionally parent scopes.
- After adding a new `.asmdef`, open the project in Unity once so generated `.csproj` files stay in sync (or rely on CI regeneration rules).

## Anti-Patterns

- Putting **`SceneFoundationScope`** in **`Game.GearEngine`** — forces non-gear scenes to depend on gear mechanics.
- Registering the scene bootstrap inside a feature **`Installer`** — couples composition to a plain installer; keep bootstrap registration in the scope.
- Duplicating **`Addressables` / `Navigation` / `Events`** installers in feature scopes — use the base class instead.

## Testing

- **Module tests**: `Assets/GearEngine/Scripts/Game/SceneFoundation/Tests/Editor/` (`Game.SceneFoundation.Tests`).
- **Commands**: run **`.agents/scripts/validate-changes.cmd`** (optionally without **`-SkipTests`** when running EditMode tests).
- **Pass signal**: `TOTAL:0` and no `BLOCKER` lines from the validation script.
- **Regressions**: any bug fix in **`SceneFoundationScope`** must add or update a failing-then-passing EditMode test per **`AGENTS.md`**.

## AI Agent Context

**Invariants**

- **`Game.SceneFoundation`** must not reference feature assemblies (**`Game.Race`**, **`Game.GearEngine`**, **`Game.CarSimulation`**, etc.).
- Derived scopes must call **`RegisterComponent`** for bootstraps with **`AsImplementedInterfaces().AsSelf()`** to preserve existing startup behavior.

**Allowed dependencies**

- **`Game.SceneFoundation`** → **`Scaffold.*`**, **`VContainer`**, **`UnityEngine`**
- Feature assemblies → **`Game.SceneFoundation`** + their domain **`Scaffold.*`** / **`VContainer`** edges

**Forbidden dependencies**

- **`Game.SceneFoundation`** → any **`Game.*`** feature assembly
- Feature **`Installer`** → registering cross-layer or global infra duplicated by **`SceneFoundationScope`**

**Change checklist**

- Update **`.asmdef`** for runtime and **Editor** if the editor touches types that inherit from **`SceneFoundationScope`**.
- Add or update **`Docs/Game/SceneFoundation.md`** when public behavior changes.
- Run **`.agents/scripts/validate-changes.cmd`**.

**Known tricky areas**

- Unity regenerates `.csproj` / `.sln` from **`.asmdef`**; solution build in VS/Rider may need a Unity refresh after new assemblies.
- **`RegisterComponent`** requires **`using VContainer.Unity`** in the same file as the scope.

## Related

- [`Architecture.md`](../Architecture.md) — modular boundaries and VContainer usage
- [`Module-Documentation-Standard.md`](../Standards/Module-Documentation-Standard.md)
- [`Race.md`](Race.md) — composed race scene (gear + car)
- [`CarSimulation.md`](CarSimulation.md) — car simulation module

## Changelog

- **2026-04-16** — Initial doc for **`Game.SceneFoundation`** and **`SceneFoundationScope`**.
