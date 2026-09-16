# Rewards return-home validation

Date: 2026-09-16. Base: `65a7ecc6`. Branch: `codex/rewards-return-home`.

## Behavior

Results already presents standings, score, and stars. The updated sequence is Results → Gold → eligible gear offer → existing gear selection → awarded gear → Home. Gold-only runs and skipped gear selections return Home directly. First place OR at least one star still grants one selection. Old Progress assets stay registered for compatibility, but these routes no longer open them.

## Checks

| Check | Outcome |
| --- | --- |
| Scoped C# lint fix and check with analyzers | Passed on the four changed C# files; workspace-loading warnings only, no style/analyzer diagnostics. |
| Focused EditMode fixtures | 12/12 passed: ResultPopupViewModelTests and PostRaceScreenTests. Includes no-gear, first-place-only, star-only, both-condition eligibility, selected/skipped gear, and repeated Continue. |
| Portrait rendering | Results and Gold captured at 1080×1680, 1080×1920, 1080×2280, and 1080×2400; gear and placement scenarios at 1080×2280. These are representative-data prefab captures, separate from the live walkthrough. |
| Repository wrapper with -SkipTests | Passed: 114 asmdefs audited, compilation exit 0, pragma gate 0, analyzer build exit 0, analyzer blockers 0. Wrapper-owned analyzer unit tests passed; broad Unity suites intentionally skipped. |
| Live app sequence | Normal Main Scene bootstrap through a real race and server rewards back to MainViewModel. First place / zero stars, 10 Gold, Quantum Link IV; one EventSystem and zero Progress views at the end. |
| Git whitespace and scope | Clean; no prefab, package, project-setting, or gameplay-rule changes. |

The two affected fixtures are the smallest relevant gate for this navigation change. No new tests were added; existing expectations now assert direct Home destinations. Live capture exercises ToolbarController; focused tests also cover the navigation fallback.

## Evidence and limits

- [Full app walkthrough](../../Artifacts/VisualTests/RewardsFlow/README.md), including observed visual issues and two runtime gear-ability exceptions whose cause was not isolated.
- [Focused test report](../../Artifacts/TestResults/RewardsFlow/Report.md) and [NUnit XML](../../Artifacts/TestResults/RewardsFlow/EditMode.xml).
- The normal loading phase was observed; retained screenshots start at Home. Actions invoked existing live button listeners through the Unity CLI. Physical touch, device cutouts, and production ad delivery were not checked.
- The temporary Pipeline package was removed after capture by restoring the original manifest and lockfile. Test/compilation gates ran with the original packages.

## Architecture

MVVM and VContainer remain intact. ReceivedRewardsViewModel uses the existing injected ToolbarController to return Home, matching RoguelikeViewModel and preserving navigation ownership. The existing Continue guard prevents repeated transitions. No new singleton, duplicated grant/persistence path, or view-owned gameplay rule was introduced.
