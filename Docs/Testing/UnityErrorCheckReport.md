# Unity Error Check Report

## 2026-07-27 — Action Invoker Row Rendering Fix

- Timestamp: `2026-07-27 09:06:00 -03`.
- Focused EditMode fixture:
  `GearEngine.GearEngine.Tests.Editor.InvokeActionEditorSelectionTests`.
- Result: 45 passed, 0 failed, 0 skipped, and 0 inconclusive.
- Evidence:
  `Artifacts/TestResults/20260727-083508/EditMode.xml`,
  `Artifacts/TestResults/20260727-083508/EditMode.log`, and
  `Artifacts/TestResults/20260727-083508/Report.md`.
- Focused Editor-log parser result: `[]`.
- Unity 6000.5.3f1 opened the worktree, compiled the final source in 9 seconds, and
  reloaded assemblies. The worktree log contains no `error CS`, `warning CS`,
  `Compilation failed`, or `Scripts have compiler errors` entries.
- Scoped C# lint completed in `fix` and `check` modes for the changed editor and test
  files. Whitespace, style, analyzers, and one-top-level-type structure checks pass.
- Agent-efficiency static verification passed with no findings.

### Fixes Applied

- Restored the populated row's explicit IMGUI header with the foldout, action display
  name, validation badge, enabled toggle, and existing weight controls.
- Kept the header outside the reorder-handle gutter and constrained collapsed rows to
  one Inspector line.
- Bound addition, first selection synchronization, height measurement, collapse, and
  drawing to the nested `actions[index].action.isExpanded` property.
- Preserved a manual collapse during repeated synchronization until the selected action
  changes.
- Made the detailed Inspector title use the selected nested action's
  `InvokeActionEditorUtility.GetDisplayName`, matching the main compact list.
- Kept the missing-action selection flow, action replacement, Execution/Order ownership,
  compact `CommandListAdaptor`, Undo behavior, and runtime serialization unchanged.
- Corrected the regression fixture to inspect the nested managed-reference property and
  added coverage for creation, first selection, repeated synchronization, collapsed and
  expanded height, explicit header ownership, and title parity.

### Verification Limitations

- The intended pre-fix regression run was blocked before NUnit execution because two
  required DLLs were still Git LFS pointer files. The worktree dependencies were hydrated
  from the shared local LFS cache before the passing post-fix run, so no pre-fix NUnit
  failure is claimed.
- The generated solution cannot be loaded by `dotnet format` because it contains duplicate
  `Unity.Multiplayer.Tools.NetStats` project names. The required checks were run against
  the two owning generated projects instead.
- Direct generated-project builds reached the execution ceiling with zero reported errors
  or warnings and were therefore inconclusive. Unity's own compiler and the focused Unity
  test run completed successfully.
- `.agents/scripts/validate-changes.sh` is environment-blocked because PowerShell 7
  (`pwsh`) is not installed; the gate was not claimed as passed.
- The worktree was registered and opened in Unity Hub, but macOS automation continued
  exposing the older main-checkout Unity window. No Inspector screenshot is claimed.
- The GUI launch log repeatedly reports a pre-existing Package Manager
  `DirectoryNotFoundException` for
  `Packages/com.unity.services.cloudcode/Samples~/CloudCodeScriptsDeployment`; it is
  unrelated to Action Invoker rendering.

### Remaining Issues

- `BlackboardWindow.Undo_ForceRepaint` can still throw a `NullReferenceException` after
  Undo/Redo. This remains an explicitly out-of-scope follow-up.
- `ItemId == -1` normalization remains unchanged and is still a separate follow-up.

## 2026-07-26 — Feature Commit Verification

- Timestamp: `2026-07-26 12:38:00 -03`.
- Checked log: `/Users/leonardosilva/Library/Logs/Unity/Editor.log`.
- Parser result: `[]` — No errors found.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.
- The repository static gate passed after scoped formatting and trailing-whitespace cleanup.
- `Assembly-CSharp.csproj --no-restore` did not complete while the Unity Editor held the generated-project compiler state. It was cancelled after five minutes without diagnostics.
- Affected generated-project builds encountered the same active-Editor compiler lock and were cancelled without diagnostics.

### Fix Applied

The feature-commit preparation pass applied the repository formatter to changed C# files
through their owning generated projects and corrected the remaining naming diagnostics in
`AnimoraActionWithTargetsDrawer`.

### Remaining Issues

The offline runtime compilation completion gate was not established while the project was
open in Unity. Source-file structure verification also reports legacy multi-type source files
and the new Blackboard partial-class files; these require a dedicated structural refactor and
were not changed as part of commit preparation.

## 2026-07-26 — Wait For Target Click Input Context

- Timestamp: `2026-07-26 11:34:00 -03`.
- Checked log: `/Users/leonardosilva/Library/Logs/Unity/Editor.log`.
- Parser result: `[]` — No errors found.
- `Scaffold.Input.csproj --no-restore`: completed with zero errors.
- `Game.GearEngine.csproj --no-restore`: completed with zero errors.
- `Game.GearEngine.Tests.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.
- Changed C# files passed scoped formatting, style, and source-file structure checks.

### Fix Applied

Wait-for-input actions now resolve the input service and its event bus as one context.
This prevents the raycaster from publishing a click through one bus while the action
listens on another in partially injected or isolated tutorial scenes. Installed input
services publish their paired global context, while only locally created fallback
services are ticked manually.

### Verification Limitation

The focused EditMode regression test could not produce NUnit XML because the project was
already open in Unity and the batch process could not initialize its licensing/database
state. The bounded run was stopped after making no progress. Its contextual report and
Editor log are under `Artifacts/TestResults/20260726-112832/`.

## 2026-07-26 — Tutorial Focus Runtime Offset Alignment

- Checked log: `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Logs/Editor.log`.
- Latest parser result: `[]` — No errors found.
- Unity imported and compiled `TutorialFocusService.cs`, `FocusPresetSOEditor.cs`, and `TutorialFocusLayoutTests.cs`.
- The Unity EditMode run executed both new offset regression tests successfully.
- Individual generated-project formatting completed for the changed C# files.
- Changed files passed source-file structure verification and `git diff --check`.

### Fix Applied

Runtime offsets no longer receive an additional target-canvas scale factor after they
have already been expressed in screen pixels. The runtime now uses center-anchored,
screen-to-canvas local positioning, while the inspector preview derives its IMGUI scale
from the same shared 20-pixel unit.

### Verification Limitation

Direct `dotnet build` commands for the generated Unity projects did not complete because
shared MSBuild processes remained locked while the Unity Editor was active. They were not
terminated to avoid disrupting the open project. Unity's own compilation succeeded, as
demonstrated by the completed EditMode run. The repository lint wrapper also cannot load
the generated solution because it contains two projects named
`Unity.Multiplayer.Tools.NetStats`; formatting was therefore run against the individual
generated projects.

## 2026-07-26 — Conditional Invoke Action Inspector Fields

- Timestamp: `2026-07-26 10:58:00 -03`.
- Checked logs: `/Users/leonardosilva/Library/Logs/Unity/Editor.log` and `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Logs/Editor.log`.
- Parser result: `[]` for both logs — No errors found.
- Unity imported `InvokeActionEditorUtility.cs`, `InvokeActionCommandEditor.cs`, and `InvokeActionPropertyVisibilityTests.cs`, and regenerated `Game.GearEngine.Tests.csproj` with the new test.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.
- `ScaffoldEditor.csproj` and `Game.GearEngine.Editor.csproj`: scoped whitespace and style formatting completed.
- All three task C# files passed source-file structure verification and `git diff --check`.

### Fix Applied

The Invoke Action inspector now delegates field visibility to `ActionBase.IsPropertyVisible` when drawing standalone actions, grouped actions, and grouped-action height calculations. This allows `ShowUIFocus` to hide its layout override fields while `_overridePresetLayout` is false and reveal them when it is true.

### Verification Limitation

The generated solution cannot be used by `dotnet format` because it contains two projects named `Unity.Multiplayer.Tools.NetStats`. Subsequent direct runtime and test-assembly builds did not complete because shared MSBuild processes remained locked while the Unity Editor was open. They were not terminated to avoid disrupting the active Editor. The focused EditMode regression test was therefore added but not executed in batch mode.

## 2026-07-23 — WaitForTargetClickAction Script Asset Name

- Fixed Unity's `ExtensionOfNativeClass` error by renaming `WaitForTargetClick.cs` and its `.meta` file to `WaitForTargetClickAction.cs`, matching the public class name while preserving the GUID.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- The renamed source passed C# formatting and source-file structure checks.

## 2026-07-23 — UIEffect Pattern Rendering Test Ambiguity

- Checked log: `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Parser result: `[]` — No compiler errors or managed exceptions found in the global log.
- Fixed six teardown calls in `UIEffectPatternRenderingTests.cs` by qualifying `UnityEngine.Object.DestroyImmediate`, removing the ambiguity with `System.Object`.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.
- The changed fixture passed C# formatting and source-file structure checks.

## 2026-07-23 — Unity 6000.5 Package Compatibility Errors

- Checked logs: `/Users/leonardosilva/Library/Logs/Unity/Editor.log` and `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Logs/Editor.log`
- Global Editor-log parser result: `[]` — No compiler errors or managed exceptions found in the global log.
- `Scaffold.Analytics.Runtime.csproj --no-restore`: completed with zero errors after replacing the removed UGS `CustomData` API with `CustomEvent` plus `RecordEvent`.
- `NaughtyAttributes.Editor.csproj --no-restore`: completed with zero errors after replacing both deprecated `GetInstanceID` calls with `GetEntityId`.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.

### Persistent Fix

`com.scaffold.analytics` now resolves from Scaffold `main`, which contains the Unity 6000.5 compatibility fix while retaining the legacy API path for earlier Unity versions. `com.dbrizov.naughtyattributes` remains embedded at `Packages/com.dbrizov.naughtyattributes` until its upstream compatibility fix is available.

### Formatting

The Scaffold Analytics source passed formatting and structure checks. NaughtyAttributes retains its upstream naming-style warnings, which are unrelated to the two corrected API calls; its changed source files pass structural checks.

## 2026-07-23 — Unity Editor Startup Blocked by Licensing IPC

- Checked logs: `/Users/leonardosilva/Documents/MatheusCohen/Gear Engine/Logs/Editor.log` and `/Users/leonardosilva/Library/Logs/Unity/Unity.Licensing.Client.log`
- Parser result for the global Editor log: `[]` — No compiler errors or managed exceptions found.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors and zero warnings.

### Root Cause

The project cannot finish opening because Unity 6000.5.3f1 requests LocalIPC protocol `1.18.1`, while the Hub-managed licensing client currently serving the generic channel accepts only `1.17.4`. The client returns response code `505` with `Unsupported protocol version '1.18.1'`.

The Editor then attempts its version-specific fallback channel, `LicenseClient-leonardosilva-6000.5.3`, but the 1.18.1 licensing client cannot acquire its `Unity-LicenseClient-leonardosilva-6000.5.3` global mutex. A stale 6000.5.3 licensing-client process is holding that mutex without providing the required channel, so the Editor times out during licensing initialization.

### Evidence

- `Logs/Editor.log`: response `505`, unsupported protocol `1.18.1`, followed by the version-specific channel timeout and licensing initialization failure.
- `Unity.Licensing.Client.log`: repeated `Failed to acquire global mutex Unity-LicenseClient-leonardosilva-6000.5.3` from client version `1.18.1`.
- Process inspection: a stale `Unity.Licensing.Client` process from Unity 6000.5.3f1 is active alongside the older Hub-managed client used by Unity 6000.3.2f1.

### Recommended Recovery

Quit the affected Unity 6000.5.3f1 Editor, terminate only its stale 6000.5.3 licensing-client process, then launch Gear Engine again from Unity Hub. Keep the Hub open; do not delete the project `Library` directory or license files. If the issue recurs after the targeted restart, quit all Unity Editors and the Hub, reopen the Hub, then open Gear Engine before any Unity 6000.3.2f1 project.

### Recovery Result

The stale 6000.5.3 licensing client was terminated and Gear Engine was launched again. The new client connected on `LicenseClient-leonardosilva-6000.5.3` using protocol `1.18.1`, resolved the Unity Personal entitlement, and initialized licensing in 0.48 seconds. The Editor progressed to `Application.AssetDatabase Initial Refresh Start`.

## 2026-07-22 — UIEffect Pattern Layer Tests

- Checked log: `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Log parser result: `[]` — No errors found.
- `Assembly-CSharp-Editor.csproj`: completed with zero errors and zero warnings.
- `Assembly-CSharp.csproj`: completed with zero errors after the Unity package-cache refresh.
- Focused Unity Test Framework execution: not started because the active project lock would conflict with the open Editor.

## Test Changes Pending Execution

- `UIEffectPatternLayerTests` is categorized as `Unit` and covers migration, fixed slot bounds, propagation, material binding, replica, append, and reset behavior.
- `UIEffectPatternRenderingTests` is categorized as `Visual` and will emit `Artifacts/VisualTests/UIEffectPatternLayers/OrderedAlphaOver.png` and `SampledTextureAlpha.png`, each with an `.evidence.json` sidecar, after a successful EditMode run.

## Remaining Issue

The Unity Editor must be closed before the focused EditMode test run can produce NUnit XML, an Editor log, and screenshot evidence without conflicting with the open project.
## 2026-07-27 — Setup BoardView Sibling Ownership

- Checked log: `/Users/leonardosilva/Library/Logs/Unity/Editor.log`.
- Latest parser result: `[]` — No compiler errors or managed exceptions found.
- `Assembly-CSharp.csproj --no-restore`: completed with zero errors.
- `Assembly-CSharp-Editor.csproj --no-restore`: completed with zero errors.
- Unity compilation precheck passed in the isolated project copy.
- Focused EditMode run passed 20/20 tests with NUnit XML and Editor-log artifacts.

### Fixes Applied

- Removed the obsolete missing `UIAudioEventTrigger` component from
  `StartRace_Button.prefab`, allowing Setup to save.
- Created `PFB_BoardView.prefab` as the Setup screen-space Board composition.
- Moved `BoardViewComponent`, `TrashDropZoneViewComponent`, and `DragOverlay` out of
  `Setup View.prefab`; Main Scene now owns the BoardView as a sibling and injects its
  reference into the Setup instance.
- Kept `GearInventoryViewComponent` in Setup and configured it to use the BoardView drag
  overlay.
- Removed two duplicate legacy-input EventSystem roots. Main Scene retains its original
  Input System EventSystem, and the regression test confirms that it is the only active
  EventSystem.
- Relinked Setup to the sibling BoardView and verified that both Canvases use the same
  event camera.

### Remaining Issues

The complete `Game.GearEngine.Tests` EditMode assembly has 26 pre-existing failures outside
the affected fixtures. The focused Board ownership, capacity, and prefab-integrity fixtures
are clean.
