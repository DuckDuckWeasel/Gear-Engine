# Victor screen migration inventory

Baseline: main `bcda1ccf`. Victor: `atlasaqui` / Victor William Monteiro da Rocha.
The `68afa7e4` → `d92a04a0` → `46ec514c` sequence is one effective change.

| Source | Source object / change | Playable destination | Disposition |
| --- | --- | --- | --- |
| `46ec514c` | Main View header, track labels and opening animation | Main View prefab | Already applied: current prefab equals `9e56d673` |
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

Validation commit `70079efe` records tests and final captures for both implementation batches. Local main integration: `611bc227`. No remote push.

The post-race correction is implemented on `codex/victor-victory-flow-fix`; its final commit and
local-main merge are recorded after validation. No remote push is authorized.
