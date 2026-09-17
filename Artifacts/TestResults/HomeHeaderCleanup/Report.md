# Home Header Cleanup Validation

## Intent

Remove the `Laps` and `Target` metadata from the Home track header. The selected track name remains visible, while the carousel, Race action, earned stars, and Best Times standings keep their existing runtime bindings.

## Regression coverage

- Test: `GearEngine.Campaign.Tests.Editor.HomePresentationTests.HomeBestTimes_ShowsSavedAndUnracedStandings`
- Result: **1 passed, 0 failed, 0 skipped**
- NUnit XML: `Green/results.xml`
- The test binds saved and unraced progress, verifies both metadata objects stay inactive after each bind, checks standings position and row count, and captures both layouts.

## Visual evidence

- `../../VisualTests/HomeStandings/HomeSavedProgress1080x1680.png`
- `../../VisualTests/HomeStandings/HomeSavedProgress1080x2280.png`
- `../../VisualTests/HomeStandings/HomeUnraced1080x1680.png`
- `../../VisualTests/HomeStandings/HomeUnraced1080x2280.png`

All four captures show the track name without `Laps` or `Target`. The selected-track controls, Race button, stars, and fitted standings panel remain inside the viewport. The unraced state still retains its fourth player row.

## Quality gates

- Scoped C# lint `fix` and `check`: passed for the presentation and regression files.
- `Game.Campaign.Tests.csproj` build: passed with **0 errors**. The generated Unity workspace reported 356 existing assembly-conflict warnings.
- `validate-changes.ps1 -SkipTests`: exited 0; assembly-reference audit, pragma gate, analyzer build, and analyzer tests passed with zero findings.
- The wrapper could not start its separate batch Editor while this worktree was already open in Unity. Compilation was instead confirmed by the live Editor regression and the successful scoped project build.

## Visual review

The 1080×1680 and 1080×2280 saved and unraced captures were inspected. No clipped text, empty standings-card area, missing runtime values, or reintroduced metadata was observed.
