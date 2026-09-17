# Home standings and gear popup

This ExecPlan is a living document.

## Purpose / Big Picture

Home Best Times must use the Results standings presentation, with rivals ranked by time and the player's saved best time. Track stars show earned score stars, independent of position. Resize the race preview and surrounding controls to fit. Fix the empty gear popup and replace its baked star sample with the selected item's actual rarity.

## Progress

- [x] Inspect starting state, existing Results rows, Home bindings, and item popup.
- [x] Bind Home standings and earned stars; adapt the layout.
- [x] Fix selected gear artwork/name/rarity and opening lifecycle.
- [x] Verify focused regressions, capture both views, run scoped lint and repository gate.
- [x] Commit and integrate locally without pushing.

## Surprises & Discoveries

Home has no tiers container assigned, so its Best Times panel is empty. Item cards have no rarity-star binding. The server stores best times only. Proceed with device-local stars after offering the optional storage choice; cloud/account-scoped persistence is a documented follow-up. Another checkout has unrelated uncommitted Home navigation work; preserve it.

## Decision Log

- Keep the best score-star count per track in device-local PlayerPrefs after an accepted server result; never infer stars from best time. Existing historical times without a saved score have zero stars.
- Home's TrackViewport had no world target. Bind its scene reference and snapshot/restore the shared track pose when leaving or interrupting Home, so preview fitting cannot alter Setup/Race geometry.
- Animora hides each player's CanvasGroup on enable. Unused parent players must be removed when replacing nested playback with explicit leaf ownership; otherwise the entire popup remains transparent. Preserve the existing sound clips.
- Interactive captures were unreliable while the Mac was locked. Use the connected headless Editor for layout inspection and the skill runner for final NUnit evidence.

Start from local main `7b43a30f` in the existing registered VictorScreensMigration worktree, on `codex/home-standings-gear-popup`. Retain MVVM, VContainer, existing navigation, and single shared board. Reuse ResultStandingsView and the same rival-time table rather than adding a new ranking rule.

## Outcomes & Retrospective

Home uses the existing Results ranking, with three score stars stored independently. The popup binds five rarity levels and owns manual animation playback. All 24 focused tests, scoped lint, compilation and the repository wrapper passed. Implementation commit `8a7319b0` is integrated into local main through `380a88e1`. No conflicts; the validated Assets tree is unchanged by the merge.

## Context and Orientation

Campaign presentation lives in Assets/GearEngine/Scripts/Game/Campaign/Presentation. MainView binds TrackStatsViewComponent. RaceStandingsModel ranks three rivals and the player; ResultStandingsView draws the rows. ItemPopupView binds an ItemSlotView. ItemSlotView currently binds icon, name, description, and rarity background but not stars. Campaign prefabs live in Assets/GearEngine/Prefabs/Campaign. TrackProgressModel and TracksClientModule expose the saved best-time snapshot.

## Plan of Work

Use the same ranking rows without a promotion animation on Home. Show an unraced player with no recorded time instead of inventing a race. Bind earned stars independently. Keep Home changes compatible with track switching. Inspect the popup's serialized card binding, transforms, and animation ownership, fix the cause, and drive rarity visuals from item data. Keep gear rarity separate from the track's three score stars.

## Concrete Steps

Inspect live prefab hierarchies with Unity CLI, modify through the Editor, and save only affected assets. Add focused failing regression assertions before repairing popup binding; update them to cover reopen/item change. Capture Home with saved and absent progress and popup with different rarities. Run scoped unity-csharp-lint fix/check, affected EditMode tests, and validate-changes.ps1 -SkipTests.

## Validation and Acceptance

Home shows three standings rows when the player is top-three, otherwise three rivals plus the player. Recorded time determines rank; saved score stars determine the three star slots. Popup shows the selected item's name, artwork, description, and correct rarity on first open, reopening, and next/previous. Capture 1080×2280 and compact 1080×1680 for affected layouts. No missing references, repeated listeners, or hidden animation state. No broad test suite.

## Idempotence and Recovery

Do not alter the other checkout or its index. Restore temporary Pipeline manifest changes after using the Editor. Abort failed merges; revert completed integration instead of rewriting history. No push.

## Artifacts and Notes

Evidence belongs in Artifacts/VisualTests/HomeStandings and Artifacts/TestResults/HomeStandings.

## Interfaces and Dependencies

Retain ITrackService and navigation contracts. Reuse Campaign presentation and GearEngine item rarity metadata. Any storage adapter must stay separate from the ranking and view code.
