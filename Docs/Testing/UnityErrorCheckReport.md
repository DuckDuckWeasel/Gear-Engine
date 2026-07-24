# Unity Error Check Report

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

The affected Git dependencies are now embedded as `Packages/com.scaffold.analytics` and `Packages/com.dbrizov.naughtyattributes`. `Packages/manifest.json` references those local package paths, so the compatibility fixes will remain after Package Manager resolves dependencies again.

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
