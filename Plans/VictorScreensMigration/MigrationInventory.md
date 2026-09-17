# Victor screen migration inventory

Baseline: main `bcda1ccf`. Victor: `atlasaqui` / Victor William Monteiro da Rocha.
The `68afa7e4` → `d92a04a0` → `46ec514c` sequence is one effective change.

| Source | Source object / change | Playable destination | Disposition |
| --- | --- | --- | --- |
| `46ec514c` | Main View header, track labels and opening animation | Main View prefab | Already applied: current prefab equals `9e56d673` |
| User-directed Home correction; existing `select_track.prefab` control | Main track selection | Migrated in `b1049ea6`: previous/next controls wrap through unlocked tracks only, refresh preview/results together, and select the displayed track when Play is pressed |
| User-directed Home correction; existing Best Times composition | Main Best Times panel | Migrated in `3d4aa546`: the visual card now fits the three visible standings rows and restores its full height for the unraced fourth row |
| User-directed Home correction; existing track header | Main selected-track header | Migrated in `8bcdb33a`: Home keeps the selected track name and deactivates the `Laps` and `Target` metadata on every bind |
| User-directed Results correction; existing standings composition | Results standings card | Migrated in `171f7ac1`: the visible `tablescore_img` card now fits the final three rows and retains its full height during the four-row promotion state |
| `574764b9` | Continue button and track selection label changes | Shared button prefabs | Already applied in shared source prefabs; reused existing button roots and callbacks |
| `5a4a8fe6` | Card artwork, stars, icon size and score panel assets | Existing Art/UI and shared UI prefabs | Already applied artwork/stars; missing item and HUD instance overrides migrated below |
| `e47af5a4` | `Container_PanelRace_new`: speed, gear, RPM and score design in source Main Scene | Race View HUD | Migrated: original speed/gear/RPM/score bindings target Victor’s HUD; segmented RPM follows the existing simulated RPM |
| `e47af5a4` | `Container_Points`: drift points/multiplier design in source Main Scene | RaceDriftScoreView presentation | Migrated: existing RaceDriftScoreView owns the new points/multiplier labels |
| `46ec514c`, `574764b9`, `e47af5a4` | AnimationAnimora BuildScreen header and panels | Setup View presentation | Already applied: header chips, title/subtitle typography and Race button match reference; current engine wording and board placement retained |
| `46ec514c`, `574764b9`, `e47af5a4` | `Results_Canvas` Photo Finish modal | ResultPopupView victory stage | Corrected: Victor's clean `PhotoFinish`/`1stPlace` labels, stats and reward panel display live time, score, laps and gold; Continue and Upgrade preserved |
| `46ec514c`, `574764b9`, `e47af5a4` | `Campaign_ReceivedRewards_new` | ResultPopupView reward stage | Corrected: dedicated received-reward screen binds gold or the selected Roguelike item's name and icon |
| `46ec514c`, `574764b9`, `e47af5a4` | `Campaign_ResultPopupView (1)` | ResultPopupView progress stage | Corrected: dedicated star/track progress screen binds achieved tier, score, time and the next unlocked track when supplied by the server |
| `bd3f8930` | UIEffect on result background, component `132848062` in AnimationAnimora | Result popup background | Migrated: patterned UIEffect copied; opening color tween now ends at the reference cream color |
| `e47af5a4` | Item_View: icon 360→260, live name Label hidden in demo, UIEffect component | Item_View prefab | Migrated: 260px icons; live labels intentionally retained; neutral UIEffect omitted |
| `e47af5a4` | Items View: card label/size overrides and component additions | Items View / Store and Garage | Migrated: 260px icons; live labels intentionally retained; neutral UIEffect omitted |
| `5a4a8fe6`, `e47af5a4` | ItemPopup View: composition and nested Container animation | ItemPopup View | Migrated: donor visual children reconnected to original view; popup icon reduced to 220px and long titles autosize to avoid overlap |
| `e47af5a4` | Changes to Roguelike scene instance | Roguelike view | Shared card artwork already applied; obsolete missing audio scripts removed; inventory, reroll and shared board preserved |
| `66ede040` | Current checkout result celebration | Result popup | Intentionally not imported: absent from main and outside Victor’s branch; original maintenance checkout remains untouched. No authorization to import the separate dependency was received. |
| `c555f825` | Additional VFX/transitions and vendor imports | None | Intentionally excluded by selected scope |
| Source commits | Recovery snapshots, audio, package changes and demo-only variants | None | Intentionally excluded |

## Integration invariants

- The main scene keeps one shared BoardView, Trash target and drag overlay.
- View/ViewModel bindings, navigation and runtime callbacks remain authoritative.
- Source demo labels must not replace live values with sample data.
- Existing prefab GUIDs remain unchanged. New assets use PascalCase and asset prefixes.
- Source animation timing is adapted only where required by current layout/ownership.

## Adaptations and ownership

- Setup’s reference demo grid and static “5/5” data are excluded. Current runtime capacity and shared board remain authoritative.
- Results is a staged flow inside the existing popup: Victory → Reward → Progress → Home.
  Upgrade opens Roguelike selection and rejoins at Reward before Progress. Demo labels are replaced
  by the actual race result or selected item.
- Victory opens before remote persistence completes, preserving current feedback timing. Continue
  and Upgrade await that persistence task before leaving Victory, so Reward and Progress never show
  stale server progression.
- Result stats-container binding was null in main; it is now assigned. Legacy scene overrides for buttons, background color and standalone time text are removed.
- Animora owns header/card entry and background color. DOTween still owns individual result rows and drift score. Popup action-button playback has one parent schedule.
- The race view temporarily reserves 20–50% of screen height for the shared board, restores its prior anchors on close/unbind, and leaves gameplay/drag ownership unchanged.
- Item names remain visible. Popup artwork and title sizing are adapted for actual multi-line names.

## Acceptance record

See `Validation.md` for actual evidence, remaining limitations and integration status. Visual provenance alone is not proof of functional acceptance.

## Migration commits

| Commit | Scope | Source provenance |
| --- | --- | --- |
| `ec497ff8` | Inventory, existing Main/Setup disposition and source/baseline captures | `46ec514c`, `574764b9`, `5a4a8fe6`, `e47af5a4`, `bd3f8930` |
| `90548be3` | Race HUD, drift labels, new reward composition, patterned background and scene bindings | `46ec514c`, `574764b9`, `e47af5a4`, `bd3f8930` |
| `855b4ba5` | Item icons, Store/Garage card presentation, item popup and missing-script cleanup | `5a4a8fe6`, `e47af5a4` |
| `0e84aec6` | Correct Victor victory modal and add Reward → Progress → Home routing | `46ec514c`, `574764b9`, `e47af5a4` |
| `b1049ea6` | Add unlocked-track navigation to Home | User-directed correction using the existing selector control |
| `3d4aa546` | Fit the Home Best Times card to its visible rows | User-directed correction using the migrated standings composition |
| `8bcdb33a` | Remove Home lap and target metadata | User-directed correction retaining the migrated track-name composition |
| `171f7ac1` | Fit the Results standings card to visible rows | User-directed correction using the migrated standings composition |
| `1bdf88b1` | Center the live Race telemetry panel, label RPM, and move the shared board below it | User-directed correction using Victor's Race HUD composition |

Home track navigation was integrated into local `main` in merge commit `a10adb73`. No remote push was performed.

The Home standings panel fit was integrated into local `main` in merge commit `b58c2425`. No remote push was performed.

The Home header cleanup was integrated into local `main` in merge commit `bc523bd3`. No remote push was performed.

The Results standings panel fit was integrated into local `main` in merge commit `7d0adf50`. No remote push was performed.

The Race telemetry layout correction is implemented in `1bdf88b1` on
`codex/race-hud-layout`. It preserves the shared board and existing simulation bindings,
adds an explicit RPM label, resets telemetry on bind, and includes active drift points
in the displayed score. It was integrated into local `main` in merge commit `de2c1537`.
No remote push was performed.

Validation commit `70079efe` records tests and final captures for both implementation batches. Local main integration: `611bc227`. No remote push.

The post-race correction is implemented in `0e84aec6` on
`codex/victor-victory-flow-fix` and merged into local main in `4901bd3e`.
No remote push is authorized.

## Superseding correction: standalone post-race screens

The previous VictoryStage/RewardStage/ProgressStage mapping was rejected by the user. These rows supersede that mapping and its completion claim.

| Source object and provenance | Destination | Status | Adaptation |
|---|---|---|---|
| `9e56d673:Assets/Lana Studio/AnimationAnimora.unity`, `Campaign_ResultPopupView/Container` (effective `46ec514c`, `574764b9`, `5a4a8fe6`, `bd3f8930`) | Existing `Campaign_ResultPopupView.prefab` and ResultPopup View/VM | Already applied; validated | Full-screen Results header, stars, score, patterned background. Real time/laps/gold replace demo ranking. No first-place claim. |
| Same source, `Campaign_ResultPopupView (1)/Container/Container_panel` | Result metric panel | Already applied; validated | Preserve authored row art, bind single-player metrics, remove demo players. |
| Same source, `Campaign_ReceivedRewards_new` | `PFB_ReceivedRewardsView`, ReceivedRewards View/VM, ViewConfig | Already applied; validated | Actual gold then optional selected gear; correct current/total count; currency/item icons. |
| Same source Results header/stars/panel; current TrackDefinition tier catalog | `PFB_RaceProgressView`, RaceProgress View/VM, ViewConfig | Already applied; validated adaptation | Separate Progress screen shows earned tiers, targets, and catalog display names. No XP or unlock transaction is fabricated. |
| `Results_Canvas`, `Campaign_ReceivedRewards_old`, alternate four-item demo | No runtime destination | Intentionally excluded | Superseded demo variants, not extra game states. |

Recovery note: source scene instances reference the campaign prefab. Recover original prefab dependencies alongside the source scene before capture; using current main dependencies contaminates the reference. Temporary snapshots are removed before commit. Authored per-object Animora clips remain; nested demo playback links are replaced by explicit screen lifecycle ownership to avoid hidden panels and competing owners.

Standalone correction implementation: `ea615d32` (`fix(campaign): implement Victor standalone post-race screens`). All superseding rows above are accounted for; the screenshot matrix and final gates accompany the evidence commit.
