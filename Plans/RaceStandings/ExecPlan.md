# Separate race standings, score stars, and gear rewards

This ExecPlan is a living document.

## Purpose / Big Picture

Results must reproduce Victor's standings panel: show the top three racers, plus the player when outside the podium. Rival names and times are synthetic, stable per track, and configurable. Lower time gives the better position; score alone gives stars. A fourth-place finish can earn three stars. Results has only Continue. The reward sequence offers Upgrade only for an eligible gear reward and opens the existing Roguelike selection.

## Progress

- [x] Preserve original maintenance checkout; start `codex/race-standings-rewards` from local main `e19f520e` in the registered migration worktree.
- [x] Reproduce the old fast-time/zero-score star bug with a focused failing test.
- [x] Implement stable standings and score-only star evaluation.
- [x] User confirmed one gear selection for first place OR at least one star, including when both apply.
- [x] Restore rank/name/time rows and animate adjacent position changes, including fourth to third and unchanged third.
- [x] Validate targeted domain/navigation/rendering scenarios, compile, lint, and repository gate.
- [ ] Commit coherent implementation/evidence groups and integrate locally; no remote push.

## Surprises & Discoveries

The old star evaluator combined time and score, and existing track assets often store score thresholds in descending order. Star presentation and local tier reward lookup now use ascending score thresholds. The broader analyzer check also exposed existing style warnings in the touched Campaign methods. Extracted race setup/persistence, reward navigation, and ad handling into named steps and retained package subscription cleanup through a local presentation adapter. The server still awards gold and progression using its existing time-band contract; do not fabricate a new server payout or unlock result in this UI change.

## Decision Log

- Replace invented time/lap/gold result rows with standings.
- Use three authored rival configurations when available; otherwise use NOVA, AXEL, BLAZE at the track's time-to-beat multiplied by 1.00, 1.12, 1.24. These are synthetic opponents, not fetched player records. Existing rivals win exact ties, preventing ambiguous placement.
- Use the previous persisted best time to establish the opening position; use this race's time for the final position. First attempts start below the podium. Unchanged third place shows only the top three and does not move. A fourth-to-third improvement swaps those rows and hides the displaced fourth row after settling.
- Gear eligibility is first place OR at least one star. One gear selection is offered even if both apply. Gold is shown once; selecting or skipping gear proceeds without restarting the queue.
- Preserve server currency grants and current unlock behavior. First-place unlock enforcement is explicitly future work requested by the user.

## Outcomes & Retrospective

Implementation and scoped verification are complete. All 46 selected cases passed across the 44 passing post-cleanup cases and the corrected three-case race fixture rerun (one overlap). Compilation and repository checks pass. See [Validation.md](Validation.md) for exact runs, limits, and evidence. Older migration screenshots showing race time/laps/gold rows are superseded by this correction.

## Context and Orientation

`RaceResultModel` captures the completed race. `RaceStandingsModel` orders the synthetic rivals and player. `TrackDefinition` owns authored rival times and score tiers. `TrackProgressModel` exposes prior best times populated by `TracksClientModule`. `ResultStandingsView` owns row movement; Animora retains header/panel animation. `ReceivedRewardsViewModel` controls the gold/gear queue; `RoguelikeViewModel` performs the existing gear selection.

## Plan of Work

Implement domain ranking and score-only stars, then replace the result rows using existing artwork. Move Upgrade into the gear reward page. Add deterministic tests for placement, ties, score/time independence, gear eligibility, queue resume/skip, and animation lifecycle. Capture current Unity rendering and run scoped checks before local integration.

## Concrete Steps

Use the existing Unity worktree and live CLI for prefab edits when reachable. Preserve prefab GUIDs and scene/navigation registration. Keep temporary Pipeline installation out of the final changes. Run changed-file C# lint fix/check, focused Unity tests with XML/log/report and screenshot evidence, and `.agents/scripts/validate-changes.ps1 -SkipTests`.

## Validation and Acceptance

Verify first/second/third/fourth placements; top-three plus player without duplicates; ties; stable rivals; fourth-to-third movement and unchanged third; poor placement with three stars; first place with zero stars; zero-star non-winner without gear; one gear selection when both criteria apply; gear skip/selection returning to progress; result-before-persistence; repeated open/close and button presses. Capture at the four portrait heights from the migration contract. Do not represent Editor evidence as a live-backend/device test.

## Idempotence and Recovery

Preserve the original checkout and server progression behavior. Abort a failed merge; revert a completed merge without rewriting shared history. No push is authorized.

## Artifacts and Notes

Initial scoped run: 42 passed, two older Roguelike fixtures failed before behavior could run because the fixtures lacked the current ad/event dependencies. Supplying those dependencies and an explicit test assembly reference made both tests pass. After analyzer cleanup, the remaining older active-race fixtures were updated for current analytics/event dependencies, deferred start, and the production Bind lifecycle. All three now pass. The final repository gate passes with zero blockers.

Evidence belongs under `Artifacts/TestResults/RaceStandings/` and `Artifacts/VisualTests/RaceStandings/`.

## Interfaces and Dependencies

Keep existing navigation and service interfaces. New ranking data remains in Campaign; serialized rival configuration belongs to CarSimulation's track definitions. `TrackProgressModel` exposes the already-persisted best-time snapshot without adding a second save path.

## Planned follow-up: first-place unlocks and reward policy

This section records future work, not behavior implemented by this correction.

1. Make the server authoritative for the same opponent-time table and finishing position; send/validate the race facts needed to reproduce the result. Client animation must never determine a grant.
2. Unlock the next track only when finishing first. Stars alone must never unlock a track. Decide whether an exact tie counts; the current standings use strict improvement.
3. Separate position rewards from star rewards in the server response. First place contributes to rewards; stars influence only rewards. Define amounts and the specific star reward types before authoring economy values.
4. Return typed reward entries and unique claim identifiers. Reconcile the current client gear rule (first OR at least one star, one pick) with server authorization so retries/reopening cannot grant twice.
5. Preserve existing best-time records, unlocks, and wallet balances during migration. Add server tests for first/second/third/fourth with zero/three stars, replay, tie, duplicate submission, and claim idempotency.
6. Replace time-band progression in `TrackRecordRaceEvaluator` and align `RecordRaceResultResponse`, track authoring, client progression, and UI only when this follow-up is implemented. No server deployment is authorized here.

## Migration commits

- `602d147b` — time-ranked standings, score-only stars, one conditional gear selection, prefab wiring, scoped regressions, and module documentation.
