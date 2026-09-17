# Home standings panel fit

## Intent

Remove the unused white area below the final visible Best Times row while preserving
the fourth row for an unraced track and keeping the shared Results standings behavior.

## Regression sequence

- Red: `Red/results.xml` — 1 failed. The three-row Home state still measured the
  original 714 px visual panel height.
- Green: `Green/results.xml` — 1 passed. The same state reduces the visual panel by
  exactly one configured row spacing, while the four-row state keeps its full height.
- Shared guard: `Shared/results.xml` — 3 passed in `PostRaceScreenTests` after the
  shared standings component changed.

The NUnit files were exported from the already-open Unity Test Runner because a second
batch Editor cannot open the same project. Unity recompiled the runtime and regression
assertion before the focused runs. A later metadata-only edit to the screenshot sidecar
criteria passed the scoped lint check and a complete `Game.Campaign.Tests.csproj` build
with 0 errors.

## Visual evidence

- `Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x1680.png`
- `Artifacts/VisualTests/HomeStandings/HomeSavedProgress1080x2280.png`
- `Artifacts/VisualTests/HomeStandings/HomeUnraced1080x1680.png`
- `Artifacts/VisualTests/HomeStandings/HomeUnraced1080x2280.png`

The saved three-row capture ends the card immediately below the player row. The unraced
capture retains the fourth placeholder row and its required panel height.

## Static gates

- Scoped C# lint `fix` and `check` passed for `ResultStandingsView.cs` and
  `HomePresentationTests.cs`.
- `validate-changes.ps1 -SkipTests` exited 0: 114 assembly definitions audited, zero
  pragma violations, analyzer tests passed, analyzer build exited 0, and zero analyzer
  diagnostics or blockers. Its batch compilation process reported that the project was
  already open; the live Editor compilation and focused test runs above are the compile
  evidence for this change.
