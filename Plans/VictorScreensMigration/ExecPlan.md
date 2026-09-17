# Migrate Victor's screen changes into the playable game

This ExecPlan is a living document.

## Purpose / Big Picture

Apply only Victor's changed screen presentation to the playable campaign. Preserve his
art and animation intent while retaining current navigation, live data, inventory,
shared board, and responsive behavior. A prefab is Unity's reusable object template;
its GUID is the stable identity used by serialized references.

## Progress

- [x] Confirm main baseline `bcda1ccfbe97d75db1fd60780a21b9c64ecd9eed`.
- [x] Create and register `codex/victor-screens-migration` in a separate worktree.
- [x] Inventory source changes against current playable prefabs.
- [x] Capture reference and current presentation.
- [x] Confirm shared Main/Setup visuals already applied; preserve current board layout.
- [x] Migrate Race/Results presentation and fix result bindings/scene overrides.
- [x] Migrate Items/Store and item popup changes.
- [x] Run repository gate and scoped checks; commit both implementation batches.
- [ ] Complete remaining acceptance coverage: device safe-area behavior, accepted pointer reposition and baseline investigation of the VariableSO finding.
- [x] Integrate migration into local main without conflicts (`611bc227`); retain documented acceptance limits.
- [x] Revalidate the corrected post-race sequence with scoped ViewModel navigation and Play-mode rendering checks to Victor's Victory → Reward → Progress → Home flow.
- [x] Replace the rejected Photo Finish popup with Victor's full-screen Results composition.
- [x] Implement separate Rewards and Progress Views, ViewModels, prefabs, and navigation configs.
- [x] Validate runtime lifecycle, bindings, and four display sizes (25 focused tests passed).
- [x] Commit the standalone correction (`ea615d32`) and its evidence.
- [x] Add Home previous/next track controls that cycle only through unlocked tracks, update the preview and saved results, and select the displayed track for play (`b1049ea6`).
- [x] Integrate the Home track carousel into local `main` without conflicts (`a10adb73`).
- [x] Remove the unused white area below the final visible Home standings row while preserving the unraced fourth-row state (`3d4aa546`).
- [x] Remove the `Laps` and `Target` metadata from the Home track header while preserving track selection and runtime standings (`8bcdb33a`).
- [x] Integrate the Home header cleanup into local `main` without conflicts (`bc523bd3`).
- Local integration: merge `codex/victor-post-race-screens` into verified `main` at `3df8b90e`; final disposition is recorded in the integration merge.

## Surprises & Discoveries

- Execution started with the original checkout clean on `codex/repository-maintenance`
  at `c5c438a4`, not main. It is left untouched.
- Celebration commit `66ede040` belongs to that branch, not main; inclusion was asked
  explicitly because the approved plan requires preserving that presentation.
- Unity `6000.5.9f1` was installed, then a dedicated headless Editor was started for this worktree. The original checkout’s Editor is untouched.
- The pinned BroAudio package needed Unity’s API updater on first compilation; `-accept-apiupdate` resolved its deprecated dropdown API in the package cache.
- The current Results prefab had a null stats container, and four affected prefabs had obsolete missing audio scripts. Red/green regression evidence is recorded.
- The shared board has its own screen-space canvas. Its race layout must be reserved/restored at the view lifecycle boundary; the legacy viewport does not move it.
- `75fdfa3f` merged `9e56d673` but preserved the playable scene and campaign prefabs.
  An ancestry check alone cannot prove visual integration.
- The original migration treated `Campaign_ReceivedRewards_new` as the result screen. The initial interpretation of the reference was rejected. The reference scene depends on campaign prefabs; migration edits contaminated earlier reference captures. Recover original objects from `9e56d673` with isolated temporary identities before comparison.
- The outlined alternate victory title produced glyph-edge artifacts after runtime text updates.
  Neither alternate title proves race placement; the game supplies tier achievements, not a finishing rank.

## Decision Log

- User selected only changed screens, design-preserving adaptive layouts, and isolation
  from main in a worktree. No remote push is authorized.
- Use current AnimationAnimora plus original source commit objects as visual references.
- Exclude `c555f825`, recovery scenes, unrelated audio, dependencies, and unchanged screens.
- Preserve MVVM (views render view-model data), VContainer dependency injection, the
  single shared BoardView, and existing navigation contracts.
- No automatic wholesale replacement of playable scenes or prefab roots.

## Outcomes & Retrospective

The original migration is on local main. The standalone correction restores Victor's full-screen Results and adds separate Rewards and Progress screens using the existing MVVM/navigation pattern. Home now cycles through unlocked tracks with the existing selector art, carries the displayed selection into Setup, Race, and result submission, and presents only the selected track name above that composition. Scoped tests and the repository `-SkipTests` gate passed; see Validation.md for exact coverage and retained acceptance limits.

## Context and Orientation

The playable entry is `Assets/GearEngine/Scenes/Main Scene.unity`. Campaign templates
live in `Assets/GearEngine/Prefabs/Campaign/`; view bindings live in
`Assets/GearEngine/Scripts/Game/Campaign/Presentation/`. The design reference is
`Assets/Lana Studio/AnimationAnimora.unity`. Navigation owns Main, Setup, Race,
Results and Roguelike; Items serves the Store/Garage variants.

## Plan of Work

1. Record every source object, destination, source commit and migration disposition in
   `MigrationInventory.md`. Compare matching runtime data before applying changes.
2. Apply shared headers/backgrounds and changed Main/Setup visuals.
3. Apply Race HUD, score and the distinct Victory, Reward and Progress presentations. Preserve
   result availability before persistence completes; block later stages until persistence finishes.
4. Apply changed cards, Items/Store layout and item popup. Keep real data bindings.
5. Validate bounded batches and integrate into main only after acceptance checks.

## Concrete Steps

- Source commits: `68afa7e4`/`46ec514c` (one effective change), `574764b9`,
  `5a4a8fe6`, `e47af5a4`, and `bd3f8930`.
- Inspect prefab overrides and reference hierarchy; preserve GUIDs and remap references.
- Use live Unity commands when reachable; inspect `unity status` first.
- Keep each animated property under one owner; reset on reopening and release listeners
  and running animations on close. Do not import obsolete demo behaviors.
- Run scoped C# lint fix/check for changed source, then compilation and affected checks.
- Record batch commits with source provenance. Recheck main before merging and rerun
  affected checks if its baseline changes.

## Validation and Acceptance

Capture actual reference/migrated views at 1080x2280, 1080x1920, 1080x2400, and
1080x1680. Exercise campaign navigation and changed item paths; verify dynamic values,
buttons, repeated opening, safe areas, board dragging and race read-only state.
Require no missing scripts/references, duplicate EventSystems, clipped controls,
duplicate listeners or stale animations. Run the repository validation wrapper with
`-SkipTests`; use focused existing tests for changed domain behavior, and regression
tests for any fixes. Never label a blocked gate as passed.

## Idempotence and Recovery

Keep unrelated work in the original checkout unchanged. Preserve existing asset GUIDs.
Abort an incomplete merge; revert a completed integration without rewriting shared
history. A source change already applied is recorded and not reimported.

## Artifacts and Notes

The inventory and validation report live beside this plan. Actual visual evidence goes
under `Artifacts/VisualTests/VictorScreensMigration/`. Temporary inspection files are
not final deliverables. Final documentation will identify commits and unverified risks.

## Interfaces and Dependencies

Public gameplay/navigation APIs remain unchanged. Any additional adapter belongs to
presentation with explicit assembly references. Keep pinned Unity/package versions.

## Current correction (2026-09-15)

Starting ref: clean local main `3df8b90e`, branch `codex/victor-post-race-screens` in the existing registered migration worktree. Original maintenance checkout remains untouched. No remote push.

The user rejected the compact victory popup and requested new screens following project examples. Use `View<T>`, separate ViewModels, Addressable prefabs and `NavigationSettings`, as used by Items and Roguelike. Results retains its prefab GUID and gameplay entry API. Rewards enumerates actual gold and optional selected gear before opening Progress. Progress shows real tier targets and earned stars, resolves next-track display names from the catalog, and returns through the toolbar. No invented leaderboard entries or first place.

Regression evidence: before implementation the new placement and navigation assertions failed (2 failed, 5 existing passed). Preserve persistence-before-advance and existing result-before-persistence behavior. Animora owns presentation animations at Open/Close; remove competing Animator components. Repeat opening and reject duplicate continuation.

Visual evidence is now under `Artifacts/VisualTests/VictorPostRaceScreens/`. Original references are recovered from Victor's git objects; runtime evidence must come from the actual bound view components after reopening. The earlier Photo Finish captures and completion claims are superseded.


### Runtime discoveries during the standalone correction

- Static rendering hid an authored scale problem: zero Z scale in animation endpoints made TMP text disappear in Play mode. The migrated endpoints now keep Z at one while preserving the 2D animation; the runtime test rejects a degenerate transform.
- The reward-counter fade animated dark text to white on a white strip. Its fade now retains the authored dark ink; a contrast assertion covers reopening.
- Opening the old source scene could regenerate nested-player references. Temporary source assets are removed, and stale Results scene overrides are cleared before final integration.
- Progress displays the tier order in the existing track catalog. Some catalog thresholds run from harder to easier; changing those gameplay definitions is outside this visual migration.
