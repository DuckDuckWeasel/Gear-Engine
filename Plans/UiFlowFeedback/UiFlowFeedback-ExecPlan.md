# Refine the track, score, star, and upgrade flows

This ExecPlan is a living document.

## Purpose / Big Picture

Apply the September 18 UI feedback to the deployed campaign flow. The home track screen must show only the top three standings, keep the player hidden until they rank in that top three, present a larger and lower track preview, make the track counter readable, and reduce the locked state to one clear disabled action. Each track must show the score target for each star and retain earned stars immediately after a race. The in-race drift callout must read `+<points> <multiplier>x`, with the green badge behind the multiplier only. Choosing a post-race upgrade must continue to the received-reward screen even if the remote roll-claim call fails after the gear has already been added locally.

## Progress

- [x] Inspect the deployed screenshots, current MVVM bindings, persistence, and reward navigation.
- [x] Add regression coverage for the requested standings, star persistence, drift callout, and upgrade recovery behavior.
- [x] Implement the scoped model, view-model, and runtime layout changes.
- [x] Run C# formatting/analyzer checks, the focused campaign tests, compilation, and visual smoke validation.
- [x] Build the verified WebGL player and stage it in the Firebase Hosting directory while preserving `PressKit/`.
- [x] Replace the storage grid's two-star design sample with the same five-level rarity binding used by the item popup, including a focused red/green regression and visual captures.
- [ ] Publish to Firebase Hosting and smoke-check the live URL after explicit confirmation of the external upload risk.

## Surprises & Discoveries

- The home and result screens share `RaceStandingsModel`, but only the home path uses `VisibleRowCount`; result animation can retain four rows while home is capped at three.
- Track score tiers already exist in `TrackDefinition`; the home view was not rendering their score targets because its tier-slot container is intentionally unassigned.
- Earned stars are stored per track, but only after a non-null server response. A locally completed race can therefore show stars on Results and lose them on Home.
- Upgrade selection adds the gear locally before claiming the remote roll. An exception from the remote claim currently prevents navigation even though the local grant already succeeded.
- The first captured home layout exposed title/star overlap at both portrait heights. Moving the title upward, the star target group downward, and the widened preview viewport lower resolved it before release.
- The broader Campaign namespace run executed 99 tests: 90 passed and 9 unrelated pre-existing/content-sync tests failed. The bounded affected set subsequently passed 48/48 with no relevant Unity log events.
- The storage card prefab had no serialized rarity-star references, leaving its authored two-filled/one-empty mockup visible. The popup supplied its own five-star override, which explained the mismatch when opening a card.

## Decision Log

- Preserve the existing visual language and use runtime layout configuration instead of introducing new prefabs or art.
- Keep four-position result animation behavior; cap only the home standings presentation through `VisibleRowCount`.
- Render compact score labels below the existing three star images using the configured tier scores.
- Treat remote roll consumption as best-effort after a successful local grant. Log failures, but complete the user-visible reward flow.

## Outcomes & Retrospective

Implementation is complete. The affected regression set passed 48/48, the corrected home visual test passed at 1080×1680 and 1080×2280, the repository compile/analyzer gate passed with zero diagnostics, and the visual artifacts show three standings rows, hidden unranked player state, separated title/stars, and per-star target scores. The later storage-card regression was reproduced red and now passes, with inspected card and popup captures both showing an Epic item as four filled stars plus one empty star. Unity rebuilt the corrected WebGL release at 38,386,118 bytes and staged its current hashed files in `Artifacts/Submission/GORn2026/GearEngineWebGL/` while preserving `PressKit/`. Firebase publication was not attempted after the external-write approval gate rejected it: the artifact contains an uncommitted working tree, including concurrent track-theme work, and requires explicit confirmation before upload to `gear-engine-gorn-2026`.

## Context and Orientation

The affected campaign UI lives under `Assets/GearEngine/Scripts/Game/Campaign/Presentation/`. Home standings come from `RaceStandingsModel` through `TrackStatsViewComponent`. Track progression is managed by `TracksClientModule` and `TrackProgressModel`. The race drift callout is bound by `RaceDriftScoreView`. Post-race upgrade confirmation flows from `ItemPopupViewModel` to `RoguelikeViewModel` and then to `ReceivedRewardsViewModel`.

## Plan of Work

1. Change home standings visibility to exactly three rows and update the existing standings/home regressions.
2. Configure the home preview bounds, counter outline, locked colors, and disabled state in `MainView`; simplify locked copy in `MainViewModel`.
3. Expose tier target scores from `TrackStatsViewModel` and render one score label beneath each star in `TrackStatsViewComponent`.
4. Record and save earned stars before the remote result call so the home screen refreshes from local progress even during service failure.
5. Shorten the drift score text and shrink/reposition the existing multiplier background at bind time.
6. Return success/failure from the upgrade pick path and continue to reward confirmation after a remote-claim exception when the local grant succeeded.

## Concrete Steps

- Edit only the campaign C# files and their existing Editor tests.
- Run `unity_csharp_lint.py` in `fix` and `check` modes for every changed C# file.
- Run the narrow campaign Editor test fixtures covering standings, home presentation, track progress, score HUD, and roguelike selection.
- Run the repository compile gate required for C# changes.
- Capture or inspect the home, locked-track, race-score, and upgrade completion states at the compact portrait viewport.

## Validation and Acceptance

- Home renders exactly three active standing rows. `YOU` appears only when the saved best time ranks first, second, or third.
- The track preview is visibly larger and lower than the deployed build; the counter has a dark outline and no longer competes with the road.
- Locked preview copy is exactly `TRACK LOCKED`; the race button is gray, labeled `LOCKED`, and non-interactable.
- Three star targets display the configured per-track score thresholds, and a completed race updates/preserves the highest earned star count even when the result response is unavailable.
- Drift feedback reads, for example, `+39 1x`; the green multiplier badge does not sit behind `+39`.
- A successful local upgrade grant opens the received-reward destination even when remote roll consumption throws.
- Storage grid cards and item popups both show one through five rarity stars from the selected item's `ItemRarity`; neither view retains a fixed two-star mockup.
- Changed C# files pass deterministic formatting/analyzer checks, focused regression tests, and compilation.

## Idempotence and Recovery

All UI configuration methods assign deterministic anchors, sizes, colors, and text styles, so repeated binds are safe. Star progress keeps the maximum earned value. Upgrade recovery never grants a second item; it only changes navigation after the first local add succeeds. If validation exposes a regression, revert the individual code slice without modifying player data or generated artifacts.

## Artifacts and Notes

User-provided evidence consists of four screenshots dated September 18, 2026: home standings/track layouts, the drift callout, and the stalled upgrade confirmation.

## Interfaces and Dependencies

No new assembly references are required. The work stays within `Game.Campaign`, uses the existing VContainer services, TextMeshPro, Unity UI, and the current navigation contracts.
