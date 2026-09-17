# Victor screens migration validation

## Scope and environment

Baseline main: `bcda1ccf`. Isolated branch: `codex/victor-screens-migration`.
Unity 6000.5.9f1, macOS, dedicated headless Editor with graphics. The temporary Unity Pipeline package was removed after capture; package manifests match main.

## Proven evidence

- Reference and before/after prefab composition renders at 1080x2280, 1080x1920, 1080x2400 and 1080x1680. Results preview uses explicitly supplied representative result rows; other prefab previews retain authored sample data.
- Runtime captures use the running campaign and actual data. The capture helper temporarily routes screen-space canvases through the rendering camera and restores them afterward. These are rendering checks, not device notch/safe-area emulation or pointer hit tests.
- Main → Setup → Race → Results ran through actual button callbacks and simulation. Results displayed completed race time, five laps, score and gold.
- Results → Roguelike → Main and Results → Main ran through actual callbacks.
- One shared BoardView and one EventSystem observed. The same board identity was interactive in Setup/Roguelike and read-only during Race (`Runtime/Flow.txt`).
- Garage opened from Main; Store opened from the toolbar. Owned inventory names, rarity artwork and descriptions appeared in the popup; next, close and reopen callbacks were exercised. No purchase was performed.
- New regression fixture: 9/9 passed, including missing-script checks for seven prefabs, result stats binding, and race board anchor restoration.
- Baseline missing-script fixture: 3 passed, 4 failed. Baseline result binding: 0 passed, 1 failed. Native NUnit XML is retained.
- C# compiled without errors after Unity API updating the pinned BroAudio package cache. No package version upgrade is part of the migration.

## Final checks and limitations

- Repository wrapper `validate-changes.ps1 -SkipTests` passed: 114 assembly definitions audited, zero pragma violations, compilation exit 0, analyzer build exit 0, zero diagnostics/blockers. Analyzer unit tests passed; Unity EditMode/PlayMode suites were skipped.
- New external asset GUIDs were checked against available asset/package metadata; see `AssetReferences.txt`.
- Scoped C# lint fix/check and the two-file structure check passed. Dotnet reported workspace-loading warnings but no formatting/style diagnostics. The live fixture run preceded the final explicit-type-only lint cleanup; the final source compiled in the wrapper.
- Drag begin/move/rejected-drop handlers were exercised with pointer event data: the drag service started and stopped and the source returned. A successful accepted reposition with physical mouse/touch input and device safe-area/notch behavior remain unverified.
- Existing `ResultPopupViewModelTests` could not exercise navigation: all three failed during container setup because `IAnalyticsService` is not registered by the existing test fixture. The production application did boot and its result buttons navigated successfully.
- Runtime reported a destroyed `Scaffold.Entities.VariableSO` reference during racing. The stack reaches unchanged `TemporaryBoostGearAbilitySO.Execute` line 23 through `QuantumLinkGearAbilitySO` and the grid update loop. No entity/gameplay source is changed by this migration. This is an unresolved runtime finding; its baseline reproduction was not established.
- Celebration commit `66ede040` is on the unrelated maintenance branch, not main. It has not been imported; the original checkout remains intact. The migration preserves the approved main baseline. Adding that separate celebration remains outside the implemented migration.

## Architecture review

- MVVM: existing ViewModels remain the only source of result, inventory and race values.
- Dependency injection: existing VContainer scene ownership and shared BoardView references are retained.
- Animation ownership: Animora handles entry/background, existing DOTween handles stat rows and drift feedback; duplicate popup button scheduling removed.
- Lifecycle: race board anchors are restored on close/unbind. Animora OnEnable playback resets and OnDisable stops/unregisters players.

## Final integration

Local main integrated the migration in merge commit `611bc227` without conflicts. Its tree exactly matched validated migration tip `70079efe` before this documentation update. Main had not moved from `bcda1ccf`; no code checks needed repeating for integration.

The original checkout stays on `codex/repository-maintenance` at `c5c438a4`, untouched and clean at the integration check. The migration worktree now checks out main. Remote main remains `bcda1ccf`; no push was performed.

Implementation and local integration are complete. Full acceptance remains qualified by the runtime finding and coverage limitations above. Recovery after integration is `git revert -m 1 611bc227`; do not reset shared history.

## Post-race flow correction

The corrected playable flow is Victory → Reward → Progress → Home. Choosing Upgrade inserts the
existing Roguelike selection between Victory and Reward, then rejoins the same Reward → Progress →
Home path. Victory still opens before persistence completes; later stages wait for persistence.

Visual evidence is under `Artifacts/VisualTests/VictorVictoryFlow/` at 1080×1680, 1080×1920,
1080×2280 and 1080×2400. Victory uses Victor's clean Photo Finish labels and live race data;
Reward uses a representative Echo gear icon; Progress uses representative server progression data.

Focused checks passed in both the live Editor and the reproducible Unity CLI wrapper:

- `ResultPopupViewModelTests`: 5 passed, including Victory → Reward → Progress → Home, Roguelike's
  return through Reward, and the persistence barrier.
- `CampaignScreenReferenceTests`: 9 passed, including all stage references and missing-script checks.
- `ActiveRaceViewModelTests.WhenResultPersistenceStalls_StillOpensResultPopup`: 1 passed.

The final NUnit XML, Editor logs and reports are retained under
`Artifacts/TestResults/VictorVictoryFlow/`. The aggregate result is 15 passed, 0 failed,
0 skipped. Unity logs contain no relevant compilation errors, exceptions or capture failures.

Scoped C# lint fix/check and the nine-file structure check passed. The final repository wrapper
`validate-changes.ps1 -SkipTests` passed its 114-assembly reference audit, pragma gate, Unity
compilation and analyzer build with zero diagnostics or blockers. The wrapper skipped its broad
EditMode and PlayMode suites as requested; the focused tests above ran separately.

Correction commit: `0e84aec6` (`feat(campaign): restore Victor post-race flow`).
Local main integration: `4901bd3e` (`merge: correct Victor post-race flow`). The merge applied
without conflicts, and local main had not moved from the verified `035e42af` baseline. No remote
push was performed.

## Superseding standalone-screen correction — 2026-09-16

This section supersedes the prior Photo Finish/stage-based correction and its screenshots. The user rejected that interpretation. Results now uses Victor's full-screen stars/score composition, followed by separate Rewards and Progress Views/ViewModels registered through Addressables and NavigationSettings. Continue returns to Main; Upgrade inserts Roguelike and then rejoins Rewards.

### Verification

- Final scoped Unity run: **25 passed, 0 failed, 0 skipped**. Includes ResultPopupViewModelTests (7), CampaignScreenReferenceTests (13), PostRaceScreenTests (2), RaceResultModelTests (2), and the existing result-before-persistence regression (1). NUnit XML, contextual report and Editor-log summary: `Artifacts/TestResults/VictorPostRaceScreens/Final/`. Full generated logs remain available locally and ignored by Git.
- The rendering coroutine enters Play mode, binds the actual three prefab Views, closes/reopens each, checks text opacity/nondegenerate scale, reward-counter contrast, viewport bounds, and single navigation after repeated clicks. It creates **12 runtime PNGs** at 1080×2280, 1080×1920, 1080×2400, and 1080×1680. Representative inspected captures include all three screens and the shortest/tallest layouts.
- Initial gate: 24 passed, 1 failed. The old RaceResultModel test used a second tier easier than the first but expected only one tier. Corrected that fixture to 15 seconds/2000 score after 30 seconds/1000 score; production scoring and reward calculations are unchanged. Initial failure evidence is retained in `InitialGate/`.
- Earlier red navigation/placement checks failed before implementation: `Red/Results.json` (live Editor result, not NUnit XML).
- Scoped C# lint fix/check passed for 13 changed C# files; structure check passed. Dotnet reports existing workspace-loading warnings, with no style diagnostics.
- Repository `validate-changes.ps1 -SkipTests`: passed 114-assembly reference audit, pragma gate, Unity compilation (exit 0), analyzer build and analyzer unit tests, zero diagnostics/blockers. Broad Unity suites were skipped. `ValidationGate.txt` records the exact output.
- Removed 24 obsolete nested-player overrides from the scene after replacing demo playback ownership. `SceneReferences.json` confirms all remaining Results prefab source IDs exist. Temporary source snapshots, Pipeline package changes, generated font atlas changes, and analyzer binary output are excluded from commits.

### Visual provenance and adaptation

Original references were recovered from `9e56d673:Assets/Lana Studio/AnimationAnimora.unity` together with the original prefab dependencies. The reference includes authored demo data; runtime screenshots use an existing three-tier track, 48.32 seconds, three laps, and 5438 score. This is a composition comparison, not a claim of identical data. See `Artifacts/VisualTests/VictorPostRaceScreens/README.md` for the screenshot matrix.

The Results rows show actual time/laps/gold instead of demo leaderboard positions. Rewards uses the named `new` composition, actual gold and optional selected item, with a live count. Progress adapts Victor's header/stars/panel to the catalog's tier targets. No placement, XP, unlock transaction, or additional reward grant is invented.

### Architecture and remaining limits

- **MVVM/navigation:** separate ViewModels own actions and runtime values; existing navigation and VContainer ownership remain in use.
- **Observer/lifecycle:** buttons detach on close; property subscriptions detach on unbind/destruction. Animora resets on opening and stops on closing. One player owns each animated object; stale nested demo links and competing Animator components are removed.
- **Shared gameplay:** board ownership, inventory/drag behavior, persistence and reward grants are unchanged.
- The scoped run isolates network, ads, and navigation destinations. It does not replace a complete live-backend campaign or physical-device safe-area check. Earlier migration acceptance limits above remain open. Existing catalog thresholds sometimes run from harder to easier; Progress reports that catalog as stored. Changing gameplay tier definitions is outside this visual correction.

Implementation commit: `ea615d32`. Integration direction: local `main` ← `codex/victor-post-race-screens`, based on `3df8b90e`; the integration merge records its two parents. Remote push is not authorized. Recover a completed integration with `git revert -m 1 <integration-merge>`, preserving shared history.

## Standings and gear eligibility correction — 2026-09-16

The [race standings correction](../RaceStandings/ExecPlan.md) supersedes the invented time/laps/gold rows and Results Upgrade action described above. Results now presents time-ranked synthetic rivals, the player's position, score-only stars, and Continue. Upgrade belongs to the gear reward page and is eligible for first place OR at least one star, with one pick even when both apply. New evidence is under `Artifacts/VisualTests/RaceStandings/Runtime/`; prior post-race screenshots document earlier iterations only.

Next-track unlocking by first place is recorded as future server work. Current backend grants/unlocks are preserved.

Final correction checks and limits: [Race standings validation](../RaceStandings/Validation.md). The original migration evidence remains historical; use the corrected capture index above for the current Results/Rewards/Progress behavior.

## Home track carousel correction — 2026-09-16

Home now uses the existing left/right selector art to cycle through unlocked tracks in
Remote Config order. Navigation wraps in both directions. The displayed track updates
the preview, name, saved score stars, and best-time standings together. Browsing alone
does not change the race; Play commits the displayed track for Setup, Race, and result
submission. Controls and the counter hide when only one track is available.

Verification:

- `MainTrackNavigationTests`: **4 passed, 0 failed**. Coverage includes first-track-only
  availability, bidirectional wrapping, selecting an earlier track for result submission,
  and refreshing Home after an unlock. Evidence: `Artifacts/TestResults/20260916-220334/`.
- Home presentation capture: **1 passed, 0 failed**, producing the updated saved and
  unraced fixtures at 1080×2280 and 1080×1680. The controls remain inside the viewport,
  and the counter no longer overlaps the RACE button. Evidence:
  `Artifacts/TestResults/20260916-220557/` and
  `Artifacts/VisualTests/HomeStandings/Home*`.
- Scoped C# lint fix/check passed for seven changed C# files.
- `validate-changes.ps1 -SkipTests` passed the 114-assembly reference audit, pragma gate,
  Unity compilation, analyzer build, and analyzer unit tests with zero diagnostics or
  blockers. Broad suites were skipped; the focused tests above ran separately.

Implementation commit: `b1049ea6`. Local `main` integration: `a10adb73` (clean merge,
parents `66d15c1f` and `0f5bece7`). No remote push was performed.

## Home standings panel fit — 2026-09-16

Home's three-row Best Times state now reduces the visual card by one configured row
spacing, keeping the card's top edge fixed and ending it immediately below the player
row. An unraced track still shows the player in fourth place and retains the complete
four-row card.

Verification:

- Red regression: **1 failed** with the old 714 px card height.
- Final Home regression: **1 passed, 0 failed** after fitting the visual panel.
- Shared `PostRaceScreenTests`: **3 passed, 0 failed**.
- Runtime captures inspected at 1080×1680 and 1080×2280 for both saved and unraced
  states. Evidence and NUnit XML: `Artifacts/TestResults/HomeStandingsFit/` and
  `Artifacts/VisualTests/HomeStandings/Home*`.
- Scoped C# lint fix/check passed for the presentation and regression files. A complete
  `Game.Campaign.Tests.csproj` build finished with 0 errors after the final metadata-only
  test edit.
- `validate-changes.ps1 -SkipTests` exited 0 with zero assembly-reference, pragma, or
  analyzer findings. Its batch compilation process could not start while the same project
  was open in Unity; the live Editor compile and focused test runs validate the gameplay
  change.

Implementation commit: `3d4aa546`. Local `main` integration: `b58c2425` (clean merge
from verified `2c333254`). No remote push was performed.
