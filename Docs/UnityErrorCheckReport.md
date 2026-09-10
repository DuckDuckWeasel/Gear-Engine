---
audience: internal
title: Unity Error Check Report
date: 2026-08-29
---

# Unity Error Check Report

The Portuguese WebGL release is live at
[gear-engine-gorn-2026.web.app](https://gear-engine-gorn-2026.web.app). The public
loader plays its video, reaches the Race screen, and the end-of-match regression
test passes when remote result persistence never completes.

## Portuguese WebGL Deployment and Match-End Check

- Timestamp: 2026-08-29 19:38:00 -0300
- Live site: `https://gear-engine-gorn-2026.web.app`
- Firebase deployment: passed; 66 hosted files released to site
  `gear-engine-gorn-2026`.
- Production WebGL build: passed and produced a 35,485,822-byte package at
  `Artifacts/Submission/Build/GearEngineWebGLPortuguese`.
- Build log: `Artifacts/Submission/Build/WebGLBuildPortuguese.log`.
- Focused EditMode regression: 1 passed, 0 failed, 0 skipped, and 0
  inconclusive.
- Regression evidence:
  `Artifacts/TestResults/20260829-RegressionFinal/EditMode.xml` and
  `Artifacts/TestResults/20260829-RegressionFinal/EditMode.log`.
- Final parser results for the WebGL build log, regression-test log, and active
  Unity Editor log: `[]` for each log.
- C# lint: scoped `fix`, `check`, and one-top-level-type verification passed for
  the changed campaign regression test. The production campaign and WebGL
  exporter files also passed their scoped lint gates.
- Browser verification: `lang="pt-BR"`, the muted loading loop played with media
  `readyState 4`, the loader reached `Tudo pronto`, and the 405 by 720 game canvas
  rendered the Race selection screen.
- Browser errors: 0. Existing warnings remain for missing legacy component
  references, a WebGL shader fallback, Unity cache responses without
  `Content-Length`, the WebGL LevelPlay fallback, and one unmatched track config.
- Match-end fix: `ResultPopupViewModel` now opens before awaiting remote result
  persistence. Persistence failures are logged without blocking or covering the
  end-of-match screen.
- Regression behavior: `WhenResultPersistenceStalls_StillOpensResultPopup`
  confirms the result popup appears while `RecordResultAsync` remains pending.
- Loading hardening: the custom template uses Portuguese copy, an optimized video
  loop, hashed filenames, Brotli with decompression fallback, data caching, and a
  Unity-to-browser ready signal so progress cannot remain stuck at 83%.

## Previous WebGL Check

The fresh WebGL package loads through the video-backed browser overlay, reaches
the playable Race screen, and completes the focused compiler, test, and log gates
with zero errors.

## WebGL Smooth Loading and Video Bootstrap Check

- Timestamp: 2026-08-29 16:46:26 -0300
- Checked active log:
  `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Checked WebGL build log:
  `Artifacts/Submission/Build/WebGLBuildSmooth.log`
- Checked focused EditMode log:
  `Artifacts/TestResults/20260829-162108/EditMode.log`
- Parser result for all three final logs: `[]`
- Focused EditMode regression tests: 3 passed, 0 failed, 0 skipped, and
  0 inconclusive.
- Fresh WebGL build: passed and produced a 35,496,337-byte package at
  `Artifacts/Submission/Build/GearEngineWebGLSmooth`.
- Final external compiler gates: `Assembly-CSharp`, `Assembly-CSharp-Editor`,
  `Game.App.Bootstrap`, `Game.GearEngine.Editor`, and
  `Game.App.Bootstrap.Tests` all passed with 0 errors.
- C# lint: focused `fix`, `check`, and one-top-level-type verification passed
  for all five changed C# files.
- Browser deployment check: the HTML loading overlay played the optimized muted
  loop while Unity downloaded and compiled, advanced to 100%, handed off to the
  playable Race screen, and produced 0 browser console errors.
- Error found and fixed: Analytics was previously initialized before Unity
  Services. The bootstrap now awaits UGS initialization before starting
  Analytics, and the bootstrap assembly explicitly references the Analytics
  runtime assembly.
- Loading hardening: the custom template uses hashed build filenames, Brotli with
  decompression fallback, data caching, a smooth staged progress indicator, and a
  Unity-to-browser ready signal so the overlay cannot remain stuck at 83%.
- Remaining issues: the generated Unity projects still report existing assembly
  reference-version warnings. They are outside this focused WebGL change and do
  not produce compiler errors.

## UI Transition Demo Scenes Check

- Timestamp: 2026-07-30 16:58:58 -0300
- Checked active log:
  `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Checked isolated capture log:
  `/tmp/ui-transition-demos.a3spVQ/Capture.log`
- Parser result for both final logs: `[]`
- `dotnet build Assembly-CSharp.csproj --no-restore`: passed with 0 errors
  against the isolated project copy.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: passed with
  0 errors against the isolated project copy.
- `dotnet build Game.GearEngine.csproj --no-restore`: passed with 0 errors
  against the isolated project copy.
- `dotnet build Game.GearEngine.Editor.csproj --no-restore`: passed with
  0 errors against the isolated project copy.
- Errors found: No errors found in the final Unity logs or compiler gates.
- Fixes applied: no source fixes were required. Parallel external builds in the
  active Unity project temporarily contended with Unity's generated `Temp/Bin`
  outputs, so the final external compiler gate was run sequentially against the
  exact isolated project copy used to generate and capture the scenes.
- C# lint: focused `fix`, `check`, and source-file structure verification passed
  for `UITransitionDemoController.cs` and
  `UITransitionDemoSceneGenerator.cs`.
- Remaining issues: generated-project warning counts include existing package and
  analyzer warnings outside this sample. No new compilation errors remain.

## Blackboard Layout and Component Inspector Check

- Timestamp: 2026-07-30 16:36:04 -0300
- Checked log:
  `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Parser result: `[]`
- `dotnet build Scaffold.VisualScripting.Editor.csproj --no-restore`: passed
  with 0 errors.
- `dotnet build Scaffold.VisualScripting.Editor.Tests.csproj --no-restore`:
  passed with 0 errors.
- `dotnet build Assembly-CSharp.csproj --no-restore`: passed with 0 errors.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: passed with
  0 errors.
- Errors found: No errors found.
- Fixes applied: no compiler fixes were required after making the Blackboard side
  panes independently resizable and replacing raw Direct-definition editing with the
  compact component Inspector.
- Remaining issues: the focused Unity EditMode test could not start because the
  project is already open in another Unity instance. Existing Unity reference-version
  warnings are outside this focused change.

## Follow-up Layout Check

- Timestamp: 2026-07-30 15:48:36 -0300
- Checked log:
  `/Users/leonardosilva/Library/Logs/Unity/Editor.log`
- Parser result: `[]`
- `dotnet build OM_TimelineCreator.csproj --no-restore`: passed with
  0 errors.
- `dotnet build Assembly-CSharp.csproj --no-restore`: passed with
  0 errors.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: passed with
  0 errors.
- Errors found: No errors found.
- Fixes applied: no compiler fixes were required after increasing the timeline clip
  height and row spacing.
- Remaining issues: existing analyzer and Unity reference-version warnings are
  outside this focused layout fix.

## Check

- Timestamp: 2026-07-30 14:55:00 -0300
- Unity version: 6000.5.3f1
- Active project log: `Logs/Editor.log`
- Parser command:
  `python3 /Users/leonardosilva/.codex/skills/unity-error-check/scripts/unity_log_parser.py Logs/Editor.log`
- Final parser result: `[]`
- Focused EditMode log: no relevant compiler errors or exceptions.

## Errors Found

The first Unity import exposed two diagnostics in the new test assembly:

- `OM_PlayState` could not be resolved by the test source.
- `OM.TimelineCreator.Editor.Tests` did not reference the runtime `OM_Shared`
  assembly that defines `OM_PlayState`.

No production assembly compiler errors were found.

## Fixes Applied

- Added the direct `OM_Shared` assembly reference to
  `OM.TimelineCreator.Editor.Tests.asmdef`.
- Added the `OM` namespace import to the focused Editor test fixture.
- Recompiled after the dependency fix and confirmed that the latest Unity
  compilation window contains no errors.

## Compiler and Verification Evidence

| Command or check | Outcome |
| --- | --- |
| Unity Editor asset refresh and recompilation | Passed after the test-assembly dependency fix. |
| `dotnet build OM_SharedEditor.csproj --no-restore` | Passed with 0 errors. |
| `dotnet build OM_TimelineCreator.csproj --no-restore` | Passed with 0 errors. |
| `dotnet build OM.TimelineCreator.Editor.Tests.csproj --no-restore` | Passed with 0 errors. |
| `dotnet build Assembly-CSharp-Editor.csproj --no-restore` | Passed with 0 errors. |
| `dotnet build Assembly-CSharp.csproj --no-restore` | Passed with 0 errors. |
| C# lint `fix` and `check` | Passed for the six changed C# files. |
| C# source-file structure verification | Passed for the six changed C# files. |
| Latest active-project log parser | `[]`. |

Generated-project warning counts include existing Unity reference/version and analyzer
warnings. The scoped lint gate reports no warnings in the changed files.

## Tests

The focused fixture
`OM.TimelineCreator.Tests.Editor.OM_TimelineInteractionTests` passed:

- Total: 5
- Passed: 5
- Failed: 0
- Skipped: 0
- Inconclusive: 0

The active Unity session contained unrelated unsaved `Main Scene` changes, so the test
was executed against a copy-on-write temporary project clone. This preserved the
user's open scene without saving or discarding it. NUnit XML, the focused Editor log,
and the generated test report are stored under
`Artifacts/TestResults/AnimoraTimelineInteraction/`.

## Remaining Issues

- No compilation errors remain in the affected or required generated assemblies.
- Existing package metadata warnings shown in the Unity Console are outside this
  change and were not modified.
