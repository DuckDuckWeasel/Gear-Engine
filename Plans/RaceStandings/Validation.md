# Race standings and gear reward validation

**Follow-up:** the user removed the redundant Progress step. The [current live walkthrough](../../Artifacts/VisualTests/RewardsFlow/README.md) shows rewards returning directly Home. The results below describe the earlier standings milestone.

Date: 2026-09-16. Base: `e19f520e`; implementation branch: `codex/race-standings-rewards`.

## Result

Results now shows time-ranked rivals and the player, score-only stars, and one Continue action. Gear selection is offered once for first place OR at least one star. The existing gear selection returns to the selected reward or skips to Progress, which returns home.

## Verification

| Check | Evidence and outcome |
|---|---|
| Regression before implementation | [Red report](../../Artifacts/TestResults/RaceStandings/Red/Report.md): fast time with zero score incorrectly earned three stars; one expected failure. |
| Scoped campaign checks after implementation and analyzer cleanup | [Post-cleanup report](../../Artifacts/TestResults/RaceStandings/PostCleanupGate/Report.md): 44 passed, two older active-race fixtures failed before their behavior ran because they lacked current dependencies. |
| Corrected race fixtures | [Active race report](../../Artifacts/TestResults/RaceStandings/ActiveRaceBound/Report.md): all three passed using the production Bind lifecycle, current analytics/event dependencies, and a nonblocking wait for the result delay. Covers deferred race start, result-before-persistence, and one currency update. |
| Final coverage | All 46 selected cases passed across the post-cleanup run and focused rerun. This is not a single all-green 46-case run. No full suite was requested or run. |
| C# | Scoped deterministic lint fix/check and analyzer checks passed for all 25 changed C# files; the final active-race test-only edit received an additional scoped fix/check. |
| Repository gate | `pwsh -NoProfile -File .agents/scripts/validate-changes.ps1 -SkipTests`: compilation PASS, assembly audit zero issues, pragma gate zero issues, analyzers zero diagnostics/blockers, analyzer unit tests passed. Unity tests are skipped by this wrapper and reported separately above. |
| Visual evidence | [Capture index](../../Artifacts/VisualTests/RaceStandings/README.md): 16 PNGs, including all three screens at 1080×2280, 1080×1920, 1080×2400, and 1080×1680. |

The focused selection covers standings, ties, score/time independence, reward eligibility, repeat presses, gear pick/skip, progress-to-home navigation, animation interruption/reopening, prefab references, and the shared board regression checks. Failed intermediate reports remain available as the correction history.

## Pattern review

- **MVVM:** ranking and reward policy remain in models/ViewModels. `ResultStandingsView` and `ResultStandingRowView` render data and animate positions; they do not grant rewards.
- **Observer lifecycle:** Results/Rewards remove button handlers and stop animations on closing. `PostRaceViewBindings` detaches the base package subscription on unbind/destruction because the current package exposes no disposal API for that subscription.
- **Animation ownership:** row positions belong to `ResultStandingsView`; Animora owns the parent panels/header. Removed competing row Animora players from the prefab.
- **Navigation and DI:** existing VContainer ownership and navigation registrations remain. Test assembly dependencies on ads and analytics are explicit. No singleton or alternate shared BoardView was introduced.

## Limits and follow-up

Captures use real Unity rendering with representative ViewModels and a render-camera canvas scale matching the authored height-based CanvasScaler. They do not certify device cutouts, physical touch input, live ads, or a live-backend campaign playthrough.

Server gold and unlock rules retain their existing behavior. First-place-only next-track unlocks, authoritative placement/reward reconciliation, typed grants, and idempotent gear claims are future work recorded in [the ExecPlan](ExecPlan.md). The current gear eligibility rule is implemented in the client reward sequence.

## Integration

Local main was rechecked before integration. The separate original checkout and its concurrent changes are preserved. No remote push is authorized or performed. Implementation and evidence are committed separately; final commit identifiers are recorded in the ExecPlan.
