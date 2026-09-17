# Gear Engine

Gear Engine is a portrait-first Unity racing prototype in which the player builds a compact gear system, watches it drive a car, earns a result, and improves the next run. It is designed for players who enjoy short arcade sessions, visible cause and effect, optimization, and roguelike upgrade choices.

The repository is also a working integration host for the modular Scaffold architecture: MVVM presentation, VContainer dependency injection, visual scripting, LiveOps, Addressables, analytics, ads, Roslyn analyzers, and source generators meet in one playable vertical slice.

> **Maturity:** active vertical-slice prototype. The main campaign loop is implemented and reproducible in the configured development environment. Online services, monetization, and some content are environment-dependent; production identity, onboarding persistence, platform release gates, maps, and ranking are not complete.

**Play the current WebGL build:** [gear-engine-gorn-2026.web.app](https://gear-engine-gorn-2026.web.app/) — version **0.1**.

Last verified against `main` and the public WebGL deployment: **2026-09-17**.

## Contents

- [Product and core loop](#product-and-core-loop)
- [Current verified state](#current-verified-state)
- [Live WebGL build](#live-webgl-build)
- [Feature inventory](#feature-inventory)
- [Architecture and data flow](#architecture-and-data-flow)
- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Configuration and services](#configuration-and-services)
- [Reproducible demos](#reproducible-demos)
- [Operational cheat sheet](#operational-cheat-sheet)
- [Troubleshooting](#troubleshooting)
- [Limitations and pending work](#limitations-and-pending-work)
- [Evidence, lessons, and Definition of Done](#evidence-lessons-and-definition-of-done)
- [Brand](#brand)
- [Contributing](#contributing)
- [License](#license)

## Product and core loop

The player is solving a readable engineering problem rather than steering every moment directly: which motor and gears should occupy a limited board, and how should they connect to produce a better run? The result screen and upgrade choice turn each race into feedback for the next configuration.

```text
Main menu
    -> browse the available tracks
    -> configure a motor and gears on the board
    -> run the current track simulation
    -> inspect score stars and the four-position time ranking
    -> collect gold and any earned gear reward
    -> choose a gear when one is awarded, then continue
    -> buy, store, merge, and reconfigure parts
    -> race again
```

The intended audience is mobile and browser players who like system-building, deckbuilding-style decisions, quick racing feedback, and incremental mastery. The current slice prioritizes the loop and its developer tooling over a finished content campaign.

### Current-build evidence

These are first-party captures registered as current-build evidence in the repository. They show implemented UI and gameplay, not concept art.

| Build workspace | Figure-eight race | Storage |
| --- | --- | --- |
| <img src="Docs/Images/T_ReadmeBuildWorkspace.png" alt="Portrait Gear Engine setup view with the gear board and parts tray" width="260"> | <img src="Docs/Images/T_ReadmeRaceFigureEight.png" alt="Portrait Gear Engine race view on the figure-eight track" width="260"> | <img src="Docs/Images/T_ReadmeStorage.png" alt="Portrait Gear Engine storage view with available gear parts" width="260"> |

## Current verified state

| Status | Meaning in this README |
| --- | --- |
| **Working** | Source, configuration, and current evidence support the behavior in the configured development environment. |
| **Partial** | A usable path exists, but persistence, UX, deployment, coverage, or production hardening is incomplete. |
| **Experimental** | A test/sample surface exists and is not part of the production campaign flow. |
| **Planned** | Documentation or concept evidence exists, but no complete runtime path was verified. |
| **Unavailable** | A previously documented path is missing or inconsistent in the current tree. |

| Area | State | Evidence and boundary |
| --- | --- | --- |
| Campaign loop | **Working** | Main, setup, active race, result, reward, roguelike choice, storage, and garage presentation code exists under [`Assets/GearEngine/Scripts/Game/Campaign`](Assets/GearEngine/Scripts/Game/Campaign). |
| Gear workspace | **Working** | Place, move, merge, delete, capacity, and saved-loadout paths are implemented under [`Assets/GearEngine/Scripts/Game/GearEngine`](Assets/GearEngine/Scripts/Game/GearEngine). |
| Race simulation | **Working** | Spline evaluation, vehicle simulation, laps, timing, and drift are separated into game modules documented in [`Docs/Game/Race.md`](Docs/Game/Race.md) and [`Docs/Game/CarSimulation.md`](Docs/Game/CarSimulation.md). |
| Track content | **Working / configured** | Fourteen authored track assets exist. Which track is currently served is controlled through the Track Remote Config rather than a hard-coded campaign map. |
| First-race tutorial | **Partial** | An authored tutorial asset and prefab are integrated into setup. Durable resume, ownership across all navigation paths, and full onboarding polish remain incomplete. |
| UGS LiveOps | **Working / environment-dependent** | Currency, inventory, loadout, perk, roguelike, and track schemas exist under [`Assets/LiveOps/RemoteConfig`](Assets/LiveOps/RemoteConfig), with Cloud Code source under [`LiveOps`](LiveOps). An authorized Unity project and environment are required. |
| Ads and analytics | **Working / environment-dependent** | Editor mock paths and runtime service integrations exist; production behavior depends on LevelPlay and UGS configuration. |
| Blackboard visual scripting | **Working** | Managed authoring, a plain C# runtime, VContainer composition, persistence adapters, and Play Mode execution controls exist under [`Assets/3rdParty/ScaffoldVisualScripting`](Assets/3rdParty/ScaffoldVisualScripting). |
| UI transition scenes | **Experimental** | Gallery and destination scenes exercise Fade, Burn, Dissolve, Square, Diamond, Stripe, Melt, and Blaze effects. They are samples, not a verified campaign-wide transition coordinator. |
| WebGL export | **Working / deployed** | The deterministic exporter builds Addressables and the main scene with the project WebGL template. Version 0.1 was built with Unity 6000.5.9f1 and deployed to [Firebase Hosting](https://gear-engine-gorn-2026.web.app/). |
| Cloud verification | **Partial** | Report-only and blocking scripts exist, but a complete verified Unity Build Automation configuration is not established by repository evidence. |
| Maps and ranking | **Planned** | Marketing and flow materials describe them, but no complete player-facing runtime was verified. |
| Legacy Cloud Code manifest path | **Unavailable** | Some older documentation refers to `Assets/CloudCode/LiveOps.ccmr` and `LiveOps/LiveOps.sln`; neither is the current deployment source. Use `LiveOps/LiveOps.Deploy.sln` for the backend build and verify deployment in the target environment. |

## Live WebGL build

The public build is available at [https://gear-engine-gorn-2026.web.app/](https://gear-engine-gorn-2026.web.app/). The loading screen displays the Unity `bundleVersion` as **V0.1**, so the running release can be identified before gameplay begins.

The 2026-09-17 release was verified through a clean isolated Unity worktree, a complete Addressables and WebGL build, and a live browser smoke check. The player reached the track-selection screen without browser console errors, the four-position Best Times panel rendered, the Press Kit remained reachable, and the root game shell was configured with no-cache headers.

## Feature inventory

### Player-facing capabilities

- **Screen-space gear board:** a portrait workspace with a motor, gear tray, capacity rules, drag-and-drop placement, movement, merging, deletion, and saved loadout recovery.
- **Observable racing:** the configured drive system feeds a spline-based car simulation with lap, time, and drift results.
- **Fourteen authored tracks:** Circle, D-Shape, Figure Eight, Hairpin, Infinity, Kidney, L-Shape, Oval, Peanut, Rounded Square, S-Curve, Star, Triangle, and Zigzag. Content availability remains server-configurable.
- **Progression surfaces:** gold, storage, garage, parts, perks, rewards, and roguelike pick/skip/reroll paths are present.
- **First-race guidance:** setup launches the authored first-race tutorial in the configured flow.
- **Portrait-first presentation:** the gameplay frame is designed for a narrow mobile canvas; wider hosts should frame it rather than stretch it.

### Developer and operations capabilities

- **Layered startup:** [`CampaignApplicationBootstrap.cs`](Assets/GearEngine/Scripts/App/Bootstrap/CampaignApplicationBootstrap.cs) composes Foundation, UGS, LiveOps, Ads, and Campaign layers.
- **MVVM and navigation:** domain state, view models, Unity views, and navigation are separate assemblies using Scaffold packages and VContainer.
- **Managed Blackboard:** reusable definitions are cloned into isolated runtimes; the editor modifies managed graphs through one Undo-aware controller. See [`Docs/ScaffoldVisualScripting/README.md`](Docs/ScaffoldVisualScripting/README.md).
- **Remote configuration:** six `.rc` definitions describe Currency, Inventory, Loadout, Perk, Roguelike, and Track data.
- **Cloud Code:** the deployable .NET solution is [`LiveOps/LiveOps.Deploy.sln`](LiveOps/LiveOps.Deploy.sln).
- **WebGL submission build:** [`SubmissionBuildExporter.cs`](Assets/GearEngine/Scripts/Game/GearEngine/Editor/SubmissionBuildExporter.cs) builds Addressables first, applies Brotli fallback/data caching/hash naming, builds the enabled main scene, and restores the previous Player Settings afterward.
- **Firebase Hosting:** [`Artifacts/Submission/GORn2026/firebase.json`](Artifacts/Submission/GORn2026/firebase.json) defines the public directory, Brotli MIME headers, and no-cache behavior for the game shell.
- **Quality tooling:** repository analyzers, source generators, change validation, and optional cloud-verification scripts live under [`Analyzers`](Analyzers), [`Generators`](Generators), and [`.agents/scripts`](.agents/scripts).
- **Git LFS:** large media and generated evidence are tracked through LFS rules; a source clone without its LFS objects is incomplete.

## Architecture and data flow

Gear Engine uses explicit assembly boundaries and VContainer composition. The dominant patterns are MVVM for presentation, Observer for bindings/events, factories for runtime construction, data-driven configuration for content, and Command/Blackboard composition for visual scripting.

```text
Unity scene and CampaignApplicationBootstrap
    -> VContainer application layers
       Foundation -> UGS -> LiveOps -> Ads -> Campaign
    -> view models and navigation
    -> domain services and local models
    -> gear-board state -> drivetrain evaluation -> car/race simulation
    -> result persistence and reward presentation

Remote Config definitions <-> authorized UGS environment
Cloud Code modules       <-> typed LiveOps client requests
Addressables             -> views and content needed by the active flow
Analytics / ads          <- campaign events and configured service adapters
```

Key repository areas:

| Path | Responsibility |
| --- | --- |
| [`Assets/GearEngine/Scenes/Main Scene.unity`](Assets/GearEngine/Scenes/Main%20Scene.unity) | Enabled campaign entry scene. |
| [`Assets/GearEngine/Scripts/App`](Assets/GearEngine/Scripts/App) | Application bootstrapping and top-level composition. |
| [`Assets/GearEngine/Scripts/Game`](Assets/GearEngine/Scripts/Game) | Campaign, race, car, cards, perks, scene foundation, and Gear Engine runtime modules. |
| [`Assets/GearEngine/Data`](Assets/GearEngine/Data) | Track, tutorial, gear, and other authored configuration assets. |
| [`Assets/LiveOps/RemoteConfig`](Assets/LiveOps/RemoteConfig) | Deployable Remote Config schemas. |
| [`LiveOps`](LiveOps) | Cloud Code solution and service-side implementation. |
| [`Assets/3rdParty/ScaffoldVisualScripting`](Assets/3rdParty/ScaffoldVisualScripting) | Managed Blackboard authoring and runtime. |
| [`Packages/manifest.json`](Packages/manifest.json) | Scaffold Git packages and Unity package pins. |
| [`Architecture.md`](Architecture.md) | Repository-wide module map and dependency policy. |

### Pattern risks to keep visible

- `ToolbarController` discovers views and toggles navigation directly, so navigation ownership is distributed instead of fully centralized.
- The result flow awaits an ad completion path without a verified timeout/cancellation boundary.
- The roguelike flow can mutate local inventory before the corresponding server consume completes, so a failed request can leave client/server state divergent.
- Setup logs the missing-motor condition, but the current path does not provide equivalent player-facing feedback.
- High-level concepts for maps, ranking, and a production transition coordinator must not be presented as implemented features.

## Prerequisites

Required for the repository and Editor path:

- Git and Git LFS.
- Unity Editor **6000.5.9f1**, the version declared by the current project.
- Authentication able to resolve the Git-based Scaffold packages in [`Packages/manifest.json`](Packages/manifest.json).
- Internet access while restoring packages and using Unity Gaming Services.
- An authorized Unity project/environment for the complete campaign service path. Never reuse production credentials in a local demo environment.

Required only for specific workflows:

- Unity CLI `unity` for the commands below. The repository was inspected with CLI `1.0.0-beta.5`.
- WebGL Build Support for the submission build.
- Firebase CLI access to `gear-engine-gorn-2026` only when an authorized hosting deployment is required.
- PowerShell 7+ (`pwsh`) for the full cross-platform validation scripts.
- A .NET SDK compatible with [`LiveOps/LiveOps.Deploy.sln`](LiveOps/LiveOps.Deploy.sln) for backend compilation.
- Unity Gaming Services CLI credentials only when an authorized deployment workflow explicitly requires them.

## Quick start

Run these commands from the repository root:

```bash
git lfs install --local
git lfs pull

unity --version
unity projects info . --format json
unity projects require . --yes
unity open .
```

`unity projects require . --yes` may download a large Editor installation when `6000.5.9f1` is missing. Omit it if the exact Editor is already installed and registered.

In Unity:

1. Wait for package resolution and script compilation to finish.
2. Confirm the Console has no compile errors.
3. Open [`Assets/GearEngine/Scenes/Main Scene.unity`](Assets/GearEngine/Scenes/Main%20Scene.unity).
4. Select the authorized development environment in **Project Settings > Services**.
5. Enter Play Mode and use the campaign flow described in Demo 1.

Do not copy account identifiers, project identifiers, service keys, access tokens, or local absolute paths into issues, logs, screenshots, or this README.

## Configuration and services

### Local environment file

The setup script reports the local prerequisites it can find and shows how to copy the optional environment-file example. It does not modify the file for you. Keep `.agents/local.env` local and never commit credentials.

```bash
./.agents/scripts/setup-environment.sh
test -f .agents/local.env || cp .agents/local.env.example .agents/local.env
set -a
source .agents/local.env
set +a
```

Edit `.agents/local.env` before sourcing it. The primary local setting is `UNITY_PATH`, pointing to the Unity executable when automatic discovery is insufficient.

### Supported environment variables

| Variable | Used by | Meaning |
| --- | --- | --- |
| `UNITY_PATH` | Local setup and validation scripts | Explicit Unity executable path. |
| `SCAFFOLD_UNITY_ALLOW_VERSION_FALLBACK` | Validation | Set to `1` only to allow a compilation fallback when the exact project Editor cannot be resolved. This does not approve a release build on another version. |
| `GEAR_ENGINE_BUILD_PATH` | WebGL exporter | Absolute or repository-relative destination resolved by the exporter. Default: `Artifacts/Submission/Build/GearEngineWebGL`. |
| `UGS_CLI_SERVICE_KEY_ID` | UGS CLI | Service-key identifier for an authorized non-interactive session. Never commit its value. |
| `UGS_CLI_SERVICE_SECRET_KEY` | UGS CLI | Service-key secret for an authorized non-interactive session. Never commit its value. |
| `CLOUD_VERIFICATION_MODE` | Cloud verification | `ReportOnly` for canary observation or `Blocking` after the gate is trusted. |
| `CLOUD_VERIFICATION_TEST_PLATFORM` | Cloud verification | `EditMode` or `PlayMode`. |
| `CLOUD_VERIFICATION_CATEGORY` | Cloud verification | Test category; defaults to `CloudVerification`. |
| `CLOUD_VERIFICATION_CATALOG_PATH` | Generated cloud-verification context | Internal catalog location populated by the automation path. Do not hand-maintain it as product configuration. |

### LiveOps and Cloud Code

Editor sign-in and UGS CLI sign-in are separate sessions. Before any remote write, verify the selected Unity project and environment in the tool that will perform that write.

Compile the current backend solution without deploying it:

```bash
dotnet build LiveOps/LiveOps.Deploy.sln -c Release
```

Remote Config authoring lives in `Assets/LiveOps/RemoteConfig`. In an authorized development environment, the repository's Editor tooling exposes **Window > LiveOps > Configs**, including Deploy and Deploy All operations. Those operations mutate remote state: review the target environment and the diff before invoking them. The repository does not establish a safe generic command that deploys both Cloud Code and all Remote Config data.

## Reproducible demos

The three scenarios below are deliberately bounded. Use a development environment and stop if package resolution or service initialization reports errors.

### Demo 1 — Complete one campaign loop

**Scenario:** prove the implemented player loop from setup through result and upgrade.

**Prerequisites:** the quick start is complete; the main scene is open; the selected development environment has the expected LiveOps content; network access is available; Game view is portrait-oriented.

1. Enter Play Mode from `Main Scene`.
2. Wait for Foundation, UGS, LiveOps, Ads, and Campaign startup to complete. The Main view should become interactive.
3. Select **Play** to open Setup.
4. Ensure a motor is on the board. Place or move available gears so they connect to the drivetrain.
5. Select **Race** and let the run finish.
6. On Results, inspect the score stars and the four-position ranking. Stars come from score; placement comes from race time, so the two outcomes can differ.
7. Select **Continue** to collect the displayed rewards. A first-place finish or at least one star awards one gear selection; both conditions together still award one selection.
8. Complete any gear reward choice, return to Main, and open **Storage** or **Garage** to confirm the resulting inventory/progression surface is reachable.

**Expected result:** setup accepts the configured board, the car runs the served track, Results separates score stars from time placement, the reward sequence completes, and the user can return to configuration/progression surfaces.

**Cleanup:** exit Play Mode. Anonymous-auth and remote service data can outlive the session; reset only the authorized development profile/environment through its supported service tooling. Do not delete production player data.

### Demo 2 — Author and run a minimal Blackboard

**Scenario:** prove that the managed visual-scripting authoring surface creates a runtime-backed graph without editing scene YAML.

**Prerequisites:** Unity has compiled successfully. Use a new unsaved scene so cleanup cannot affect campaign content.

1. Create a new empty scene and choose **GameObject > Scaffold > Blackboard**. This creates a Blackboard host and opens the managed editor.
2. In the graph canvas, choose **Add Block** and select the new Block.
3. In the detail panel, add a **Blackboard Enabled** trigger.
4. Add an action track if the Block does not already have one, select **Add Action**, choose **Scripting > Debug Log**, and set its message to `README_BLACKBOARD_OK`.
5. Open the Console, enter Play Mode, and wait for the Blackboard to enable.
6. Confirm that `README_BLACKBOARD_OK` appears once for the enabled execution path. Use the editor's runtime controls to inspect the selected Block if needed.

**Expected result:** the wrapper constructs a managed Blackboard runtime, the enabled trigger executes the Block, and the configured log message appears without adding graph-node components.

**Cleanup:** exit Play Mode and close the unsaved scene without saving. If you deliberately saved a definition asset, remove only that known asset through Unity so its `.meta` file is handled with it.

![Managed Blackboard editor with graph canvas and detail panel](Docs/Images/T_ReadmeBlackboardEditor.png)

### Demo 3 — Build the WebGL submission locally

**Scenario:** produce a local, non-deployed WebGL build using the repository exporter.

**Prerequisites:** Unity `6000.5.9f1`, WebGL Build Support, restored LFS/package content, successful compilation, required service configuration, and no GUI Editor currently holding the project open.

```bash
export GEAR_ENGINE_BUILD_PATH="$PWD/Artifacts/Submission/Build/GearEngineWebGL"

unity build . \
  --editor-version 6000.5.9f1 \
  --target WebGL \
  --execute-method GearEngine.GearEngine.Editor.SubmissionBuildExporter.BuildWebGl \
  --output-path "$GEAR_ENGINE_BUILD_PATH" \
  --log-file "$PWD/Artifacts/Submission/Build/GearEngineWebGLBuild.log" \
  --no-tail
```

1. Run the command from the repository root.
2. Watch the specified log for Addressables and player-build errors.
3. Confirm the destination contains a WebGL `index.html` plus its generated build data.
4. Serve the directory through an HTTP server appropriate for Brotli content; opening `index.html` directly from `file://` is not a valid browser acceptance check.

**Expected result:** Addressables build first, the enabled main scene builds with the `PROJECT:GearEngine` template, and the exporter reports the resolved output directory and byte count. Nothing is uploaded or published.

**Cleanup:** run `unset GEAR_ENGINE_BUILD_PATH`. After confirming the resolved path is exactly the disposable build directory above, remove it through Finder/Trash or another recoverable file operation. Keep the log when investigating a failure.

### Publish the verified WebGL build

Publishing is an external write. Confirm the Firebase account and target project before running these commands:

```bash
rsync -a --delete --exclude PressKit/ \
  "$GEAR_ENGINE_BUILD_PATH/" \
  Artifacts/Submission/GORn2026/GearEngineWebGL/

cd Artifacts/Submission/GORn2026
firebase deploy --only hosting --project gear-engine-gorn-2026
```

The `PressKit/` exclusion preserves the existing public press kit while replacing stale player files. After deployment, load the root URL without a query string, confirm the loading screen reports the expected version, wait for the game to become interactive, and check that the browser console has no errors.

## Operational cheat sheet

### Preflight and inspection

```bash
git lfs status
unity --version
unity projects info . --format json
unity editors running --format json
unity status --format json
unity pipeline list --format json
```

- `unity status` reports only reachable Pipeline-enabled GUI Editors. An open Editor without the Pipeline package may still appear in `unity editors running`.
- `unity pipeline list` distinguishes a missing package from Safe Mode and an unreachable server.
- Inspect the active scene, target environment, and Build Settings inside Unity before Play Mode, remote writes, or a player build.

### Start and stop

```bash
unity open .
```

Stop Play Mode from Unity before changing service configuration or closing the Editor. Save intended scene/asset changes, then close this project through the Unity UI. Do not terminate every Unity process by name; another project may contain unsaved work.

### Logs and static quality checks

Filter the narrow project log rather than dumping unrelated global Editor logs:

```bash
rg -n -i 'error CS[0-9]{4}|Scripts have compiler errors|Exception:' Logs/Editor.log | tail -80
```

For documentation-only changes, the proportionate repository check is:

```bash
./.agents/scripts/validate-changes.sh -SkipTests
```

For C# work, follow [`AGENTS.MD`](AGENTS.MD), run the changed-file lint workflow, fix compilation errors, and use the narrowest warranted test selector. Analyzer-only inspection is available as:

```bash
pwsh -NoProfile -File .agents/scripts/check-analyzers.ps1
```

### Recovery and cleanup

If a visible Editor cannot be reached through Pipeline tooling:

1. Run `unity pipeline list --format json` and inspect its Safe Mode and package fields.
2. If Safe Mode is reported, filter `Logs/Editor.log` for `error CS####`, fix only the reported source errors, and restart that specific project.
3. If the Pipeline package is absent, use normal Editor operation; install Pipeline only when live CLI control is actually required.
4. If package restoration is incomplete, confirm Git credentials and rerun `git lfs pull` before deleting caches.

To preview cleanup of regenerable Unity caches after the Editor is closed:

```bash
unity projects clean . --dry-run
```

Run `unity projects clean .` only after reviewing the dry run and confirming the project is closed. Do not remove `Assets`, `Packages`, `ProjectSettings`, `LiveOps`, `Docs`, `Artifacts`, or uncommitted user work as cache cleanup.

## Troubleshooting

| Symptom | Likely cause | Action |
| --- | --- | --- |
| Git package authentication or resolution fails | The Scaffold Git dependencies cannot be accessed, or credentials are stale. | Verify repository access without printing tokens, then allow Package Manager to resolve [`Packages/manifest.json`](Packages/manifest.json) again. |
| Textures, videos, PDFs, or evidence files are tiny pointer text | Git LFS objects were not fetched. | Run `git lfs install --local` and `git lfs pull`. |
| Unity opens with another version | The exact project Editor is missing or not registered. | Inspect `unity projects info . --format json`, install/register `6000.5.9f1`, and reopen. Do not accept another version as a release gate merely because it compiles. |
| `unity status` says no instances while Unity is open | The project does not have a reachable Pipeline-enabled GUI Editor. | Compare `unity editors running` with `unity pipeline list`; normal GUI use does not require Pipeline. |
| Unity enters Safe Mode | One or more C# compile errors prevent packages from loading. | Filter `Logs/Editor.log` for compiler errors, fix the reported source, restart the affected Editor, and recheck. |
| Main view never becomes interactive | Startup failed in UGS, LiveOps, Ads, Addressables, or Campaign composition. | Inspect the project Console/log, confirm the selected development environment and network access, and fix the first startup error rather than bypassing a layer. |
| Race cannot start | No motor is present or the configured loadout/content is invalid. | Return to Setup, place a motor, inspect board capacity/connections, and check Track/Loadout Remote Config. |
| Roguelike inventory appears inconsistent after an error | The client may have changed local inventory before a failed server consume. | Stop the flow, inspect the request error, and refresh from the authoritative service state; do not continue spending from the stale view. |
| Result flow waits indefinitely around an ad | The current completion path has no verified timeout/cancellation boundary. | Use the Editor/mock path for diagnosis, inspect LevelPlay callbacks, and restart the bounded session. Treat this as an implementation limitation. |
| WebGL build fails before player export | Addressables, WebGL support, compilation, or the custom template is missing. | Read `GearEngineWebGLBuild.log`, fix the first reported error, and rerun the same exporter. |
| Browser cannot open the local WebGL build | The build was opened from `file://` or the server does not handle Brotli/fallback files. | Serve the output over HTTP with the correct MIME/encoding behavior. |
| Old LiveOps instructions reference missing files | Documentation predates the deploy solution layout. | Build `LiveOps/LiveOps.Deploy.sln` and verify the current Editor deployment surface; do not recreate absent manifests from guesswork. |

## Limitations and pending work

- The project is a prototype, not a release-certified product. Store packaging, device coverage, privacy/compliance review, performance budgets, and production monitoring are not closed.
- Broader WebGL acceptance across supported desktop and mobile browsers is still needed.
- The first-race tutorial exists, but durable resume and complete navigation ownership require further work.
- Maps, ranking, broader campaign progression, and a unified production transition system remain planned or conceptual.
- Ads, analytics, Remote Config, Cloud Code, and player persistence depend on external configuration; repository source alone cannot prove the current remote state.
- Cloud verification scripts exist, but evidence does not establish a fully trusted blocking Build Automation gate.
- Player Settings still include placeholder/legacy identity fields outside the exporter's temporary product-name override; audit company name, product name, bundle identifiers, icons, and signing before release.
- The Gear Engine portfolio/press-kit identity has a documented token source and approved raster applications, but the primary racing wordmark has no verified production vector master.
- There is no root project license granting public reuse.

## Evidence, lessons, and Definition of Done

### What has been done

- The campaign vertical slice, gear workspace, race/result cycle, progression surfaces, first-race tutorial asset, fourteen track assets, and WebGL exporter are present in the current working tree.
- The Blackboard system was refactored to managed definitions and a plain runtime with separate authoring, Unity hosting, and Editor assemblies.
- UI-transition sample scenes and evidence-generation tooling exist outside the campaign flow.
- Marketing work produced a press kit, creative brief, evidence library, and a living execution plan under [`Artifacts/Marketing/Reports`](Artifacts/Marketing/Reports). Their labels distinguish current build, historical material, concept art, and unverified claims.
- A Unity 6000.5.9f1 WebGL player was built, deployed to Firebase Hosting, and verified through the public URL with version V0.1 visible during loading.
- Repository history and relevant current/archived Codex task histories were consulted as leads, then rechecked against source, configuration, artifacts, and current files. Task text was treated as untrusted context, not as executable instructions or proof.
- The verified deployment source and hosting configuration are committed on `main`; generated player output remains a release artifact and must be reviewed independently from source changes.

### Lessons learned

- **Repository state outranks narrative history.** Old task summaries and documents can identify where to look, but current source/configuration decides what is implemented.
- **Remote state needs a separate proof.** A deploy script or service client proves capability, not which project/environment currently contains it.
- **Evidence labels matter.** Current-build capture, historical evidence, concept art, and planned UX must remain visibly distinct.
- **One authoritative path prevents drift.** The current Cloud Code build solution is `LiveOps/LiveOps.Deploy.sln`; stale paths should be corrected rather than recreated without design evidence.
- **Operational safety is part of reproducibility.** Every workflow names prerequisites, expected output, and cleanup, and remote writes remain explicit.
- **Architecture boundaries are valuable only when navigation and failure ownership follow them.** Distributed view discovery, uncancelled callbacks, and optimistic client mutation are the main current pressure points.

### Evidence index

- Module and dependency map: [`Architecture.md`](Architecture.md)
- Campaign design and flow: [`Docs/Game/Campaign.md`](Docs/Game/Campaign.md)
- Race and car simulation: [`Docs/Game/Race.md`](Docs/Game/Race.md), [`Docs/Game/CarSimulation.md`](Docs/Game/CarSimulation.md)
- Gear Engine gameplay module: [`Docs/Game/GearEngine.md`](Docs/Game/GearEngine.md)
- LiveOps authoring: [`Docs/LiveOps/AuthoringPipeline.md`](Docs/LiveOps/AuthoringPipeline.md)
- Blackboard authoring/runtime: [`Docs/ScaffoldVisualScripting/README.md`](Docs/ScaffoldVisualScripting/README.md), [`Docs/ScaffoldVisualScripting/BlackboardRuntime.md`](Docs/ScaffoldVisualScripting/BlackboardRuntime.md)
- Testing policy and cloud verification: [`Docs/Testing/Testing.md`](Docs/Testing/Testing.md), [`Docs/Testing/CloudVerification.md`](Docs/Testing/CloudVerification.md)
- Current/historical marketing evidence: [`Artifacts/Marketing/Data/EvidenceManifest.json`](Artifacts/Marketing/Data/EvidenceManifest.json), [`Artifacts/Marketing/Data/HistoricalSourceIndex.json`](Artifacts/Marketing/Data/HistoricalSourceIndex.json)
- Product-direction boundary: [`Artifacts/Marketing/Data/ProjectDirectionManifest.json`](Artifacts/Marketing/Data/ProjectDirectionManifest.json)

### Measurable Definition of Done for this guide

- [x] Product purpose, audience, problem, maturity, and core loop are stated.
- [x] Working, partial, experimental, planned, and unavailable capabilities are separated.
- [x] Setup, service configuration, exact commands, operational recovery, and known environment variables are documented without secret values.
- [x] Exactly three bounded scenarios include prerequisites, numbered steps, expected results, and cleanup.
- [x] README visuals are local, first-party, and labeled as current-build evidence; identity references point to the existing Gear Engine portfolio/press-kit kit.
- [x] Architecture, data flow, design patterns, anti-patterns, limitations, contribution rules, and license status are visible.
- [x] Local Markdown links, image paths, code-fence balance, privacy patterns, and asset formats are checked after editing.
- [ ] A maintainer runs the three scenarios in a clean clone and records acceptance results for the intended development environment.
- [ ] Product owners approve release scope, external-service configuration, legal identity, and distribution terms.

## Brand

The existing Gear Engine identity belongs to the portfolio and public press-kit work. Its current guide is [`Docs/BrandKit.md`](Docs/BrandKit.md), its canonical machine-readable values are in [`Artifacts/Marketing/Brand/BrandTokens.json`](Artifacts/Marketing/Brand/BrandTokens.json), and its browser-ready presentation is available in the repository at [`Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandKit.html`](Artifacts/Submission/GORn2026/GearEngineWebGL/PressKit/BrandKit.html) and on the [public site](https://gear-engine-gorn-2026.web.app/PressKit/BrandKit.html).

The game identity uses Asphalt `#0B0F14`, Pit Wall `#151C24`, Race White `#FFF8E8`, Racing Red `#EF3E2F`, Victory Yellow `#FFD43B`, Track Blue `#18AEEA`, and Grass Green `#43B649`, with Oswald Variable for display text and Inter Variable for body text. The source invariant is [`Assets/GearEngine/Art/Splash Screen/Game Icon.png`](Assets/GearEngine/Art/Splash%20Screen/Game%20Icon.png); the approved racing-logo applications are preserved under [`Artifacts/Submission/GORn2026/GearEnginePublicPressKit/Logos`](Artifacts/Submission/GORn2026/GearEnginePublicPressKit/Logos).

The Leonardo Lycan lockup used by portfolio reports is an owner/portfolio endorsement asset, not the Gear Engine game logo. The tracked public press-kit files still require normal provenance review before being treated as release masters. No application identity or runtime palette was changed by this README update.

Use the WebGL press-kit copy for a local browser preview because its adjacent `Fonts` and `Media` directories satisfy the document's relative asset paths. The source copy under `GearEnginePublicPressKit/Brand` currently has no adjacent `Fonts` directory and therefore falls back to system typography.

## Contributing

Read [`AGENTS.MD`](AGENTS.MD), [`Architecture.md`](Architecture.md), and the relevant module documentation before changing code. Plans for significant features/refactors belong under [`Plans`](Plans) and must follow [`PLANS.md`](PLANS.md).

Keep changes minimal and inside assembly boundaries. Declare dependencies through `.asmdef`/project configuration, use VContainer for composition, preserve Unity-free assemblies, handle async/service errors explicitly, never add `ConfigureAwait`, and do not add warning suppressions without approval. Bug fixes require regression tests; other test creation and execution follows the repository's proportionate verification policy.

The former root package catalog is still useful when consuming Scaffold outside this project. Use [`Docs/ConsumingScaffoldPackages.md`](Docs/ConsumingScaffoldPackages.md) and [`Docs/NewProjectFromScaffold.md`](Docs/NewProjectFromScaffold.md) for that workflow rather than treating Gear Engine as only a package index.

## License

No root `LICENSE` file is present, so the repository does not currently grant a public license for copying, modification, or redistribution. Third-party components retain their own licenses. Obtain explicit permission from the repository owner before reusing project code or assets, and complete a license/provenance review before distribution.
