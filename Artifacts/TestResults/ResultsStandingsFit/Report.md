# Results Standings Card Fit Validation

## Intent

Remove the unused white area below the last visible Results standings row while preserving the four-row rank-promotion animation.

## Root cause and correction

`ResultStandingsView` was resizing its `Score` transform because the visible white card is a sibling rather than its graphical parent. The Results prefab now explicitly assigns `tablescore_img` as the resizable panel. Home keeps the existing automatic parent fallback.

With four visible rows, the card retains its authored height. When Results settles on three rows, the visual card height decreases by one configured row spacing and keeps its top edge fixed.

## Regression evidence

- Test: `GearEngine.Campaign.Tests.Editor.PostRaceScreenTests.PostRaceScreens_OpenCloseAndRenderBoundData`
- Red: **0 passed, 1 failed** because the selected panel was `Score` instead of `tablescore_img`.
- Green: **1 passed, 0 failed, 0 skipped**.
- Red NUnit XML: `Red/results.xml`
- Green NUnit XML: `Green/results.xml`

The regression verifies the visual-panel identity, the expanded four-row state, the final three-row height, repeated opening, rank animation completion, bindings, and viewport bounds.

## Visual evidence

- `../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1680.png`
- `../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x1920.png`
- `../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2280.png`
- `../../VisualTests/RewardsFlow/Portrait/Campaign_ResultPopupView1080x2400.png`
- `../../VisualTests/RewardsFlow/Portrait/FourthBeforePromotion1080x2280.png`

The four final Results captures end the white card after the third row. The pre-promotion capture retains all four rows without clipping.

## Quality gates

- Scoped C# lint `fix` and `check`: passed for the presentation and regression files.
- `Game.Campaign.Tests.csproj` build: passed with **0 errors**. The generated Unity workspace reported existing assembly-conflict warnings.
- `validate-changes.ps1 -SkipTests`: exited 0; assembly-reference audit, pragma gate, analyzer build, and analyzer tests passed with zero findings.
- The wrapper could not start a second batch Editor while the worktree was open in Unity. Compilation was confirmed by the live Editor regression and successful scoped project build.
