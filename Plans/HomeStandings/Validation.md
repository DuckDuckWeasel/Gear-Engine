# Home standings and gear popup validation

## Scope

Local branch `codex/home-standings-gear-popup`, based on `main` at `7b43a30f`. No package upgrades, remote push, server schema changes, or changes to the other checkout are included.

## Behavior

- Home reuses Results' three rival times and saved player time. Top-three players occupy their row; otherwise the player appears below the three rivals. Unraced players have a placeholder time.
- The three track stars retain the highest score-star count independently of time. Accepted results save stars on this device; cloud/account-scoped storage is a follow-up. Legacy saved times do not contain a recoverable score.
- The smaller Home track preview fits below the header and above Race. Its shared transform is restored on close/hide/disable, including interrupted transitions.
- Popup title, icon, description and five rarity slots bind to the selected gear. Epic fills four slots; Common fills one. Next/previous and repeated opening are covered. Titles keep the authored dark color for contrast.

## Verification

- Focused Unity EditMode run: **24 passed, 0 failed, 0 skipped**. Selector: `HomePresentationTests;TrackGameDataTests;RaceStandingsTests`.
- Scoped C# formatter fix/check: passed.
- Repository wrapper `validate-changes.ps1 -SkipTests`: passed. Unity compilation passed; asmdef audit and pragma gate reported zero findings; analyzers reported zero diagnostics/blockers. The wrapper also passed its required analyzer unit tests.
- `git diff --check`: passed.
- Temporary Pipeline package changes were restored. The wrapper-generated analyzer binary was restored to its original version; no analyzer source changed.

Evidence index: `Artifacts/VisualTests/HomeStandings/README.md`.
Final NUnit XML, Editor log and contextual report: `Artifacts/TestResults/HomeStandings/Final`.

Regression evidence:

- The initial popup had no bound rarity-star array: expected five, actual zero (`RarityRegressionBeforeFix.json`).
- Running the shared-preview regression against the original MainView fails because leaving Home retains `(0, 2.42, 10)` instead of `(1, 2, 3)` (`PreviewRegressionBeforeFix.json`).
- Interactive and headless investigation found Animora's enable-time CanvasGroup hiding. The popup now removes unused parent players and uses one explicit owner for each animated leaf.
- The track persistence fixture needed its existing analytics/event-bus dependencies registered to exercise CurrencyClientModule. This only changes test setup.

## Visual review

Live scene: application startup → Home → Setup → Home. Home captured at 1080×2280 and 1080×1680; Setup captured as a spot check. Its pre-existing header/preview arrangement is unchanged. No full race replay was required for this follow-up: reward navigation and race rules are unchanged, and shared-track restoration is verified separately.

Presentation fixtures: unraced player, third place with two score stars, and the real Quantum Link IV card at both sizes. The popup fixture exercises next/previous and reopening. Isolated Home fixture captures omit world geometry/toolbar; live-scene captures include them.

Both affected prefabs have zero missing scripts. Required bindings are exercised by the fixtures. No claim of a repository-wide missing-reference audit is made.

## Architecture review

- **MVVM:** ranking, star retention and item rarity remain model/ViewModel data; views bind values and actions.
- **Animation lifecycle:** popup animation start/stop is owned by its view adapter. Home owns/cancels its preview coroutine and tween and restores the shared track before another screen uses it.
- **Dependency injection:** existing VContainer ownership is retained; the test assembly explicitly declares its FrustumFit dependency.
- **Persistence boundary:** score stars are device-local pending a future backend contract; no best-time inference or invented historical score is used.

## Local integration

Implementation: `8a7319b0`. Merge into local main: `380a88e1`. Refreshed origin/main was already an ancestor of local main; no intervening upstream changes required reconciliation. The Assets tree matches the validated feature commit exactly, so no duplicate test run was needed. The original checkout on `codex/repository-maintenance` and its pending work were left untouched. No remote push.
