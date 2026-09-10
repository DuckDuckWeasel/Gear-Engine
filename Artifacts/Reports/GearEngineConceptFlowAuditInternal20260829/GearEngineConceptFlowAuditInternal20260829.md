---
title: Gear Engine Concept Flow Audit
audience: internal
date: 2026-08-29
status: review
artifact_type: report
language: en-US
---

# Gear Engine Concept Flow Audit

The concept set now has a distinctive, coherent product voice: warm paper, graphite structure, orange/cyan emphasis, tactile gear pieces, and compact racing dioramas. It is credible as press-kit concept art. It is not yet a coherent production UI system. The most important next step is to align every screen to one playable loop and make error recovery visible before adding more surface area.

## Executive summary

**Overall assessment:** strong art direction, incomplete interaction model.

The implemented campaign is a short five-stage loop:

`Main → Setup → Active race → Result → Reward choice → Main`

The seven concepts imply a broader product:

`World tour → Region → Track → Setup → Race → Result → Reward → Ranking`

That broader model can work, but ranking, world-map navigation, local-map selection, placement, lock states, and track selection are not represented by the current campaign contracts. They should remain explicitly labeled roadmap concepts until those contracts exist.

Three issues should be fixed before this direction becomes production UI:

1. A result-upload failure can prevent the result screen from opening after a finished race.
2. A full inventory prevents reward selection with only a log message and no player recovery path.
3. Setup can reject a race because the board lacks a motor, again with no visible explanation or guided recovery.

The recommended target is a resilient six-stage loop with optional secondary views:

`Tour/Main → Track setup → Race → Immediate local result → Reward → Tour/Main`

Ranking is a secondary detail from a track or result. The world and local map should initially be one responsive Tour screen; split them only when the content hierarchy earns the extra navigation step.

## Scope and evidence

This audit covers the seven canonical press-kit concepts and the current Campaign implementation.

| Concept | Intended role | Audit status |
|---|---|---|
| [Product vision, landscape](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineProductVisionLandscape.png) | Hero explanation | Keep as press-kit art |
| [Product vision, portrait](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineProductVisionPortrait.png) | Mobile loadout | Restructure before UI production |
| [Ranking](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineRankingConcept.png) | Best-times view | Roadmap concept |
| [Reward](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineRewardConcept.png) | Post-race choice | Closest to implementation |
| [Match result](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineMatchResultConcept.png) | Race outcome | Revise data and actions |
| [World map](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineWorldMapConcept.png) | Campaign overview | Roadmap concept |
| [Local map](/Users/leonardosilva/Documents/MatheusCohen/Gear%20Engine/Artifacts/Marketing/Evidence/GeneratedConcepts/T_GearEngineLocalMapConcept.png) | Track selection | Roadmap concept |

Implementation evidence includes the Campaign flow documentation and the Main, Setup, Active Race, Result, Roguelike, Track, and Toolbar presentation code. This is a static review. It does not claim device behavior, runtime accessibility, input latency, safe-area compliance, or final contrast values.

## Current flow

The current code has one selected track, a saved board layout, a race simulation, a result popup, and a post-race perk choice. The result model includes time, laps, drift score, tier, gold, and good-result state. It does not include race placement. Track persistence stores ordered tracks, the current track, and best time per track, then advances to the server-provided next track.

The current navigation graph is:

```text
Main
  ├─ Play → Setup → Active race → Result
  │                              ├─ Upgrade → Reward choice → Main
  │                              └─ Continue → Main
  ├─ Store → Items
  └─ Garage → Items
```

The press-kit set communicates a different graph:

```text
World map → Local map → Setup/race preview → Race
                                      ↓
Ranking ← Result → Reward
```

The gap is not visual polish. It is product structure: selection ownership, progression rules, data sources, offline behavior, and recovery states need explicit contracts.

## Prioritized findings

| Priority | Area | Finding | Recommended change |
|---|---|---|---|
| P0 | Race completion | `ActiveRaceViewModel` waits for `RecordResultAsync` before opening the result. Its catch path only logs, so a network failure can strand a completed race. | Open the locally computed result immediately. Sync in the background, show `Syncing…` or `Result saved locally`, and expose retry without blocking continuation. |
| P0 | Reward | A full inventory produces `TODO: Show visual warning - Inventory Full` and stops selection. | Preserve the roll and show a persistent inline state with `Open Storage` and `Skip` actions. Return to the same choices after storage management. |
| P0 | Setup | Starting without a motor logs an error and returns. | Disable `RACE` until the board is valid, explain `Add a motor to race`, and focus or pulse the motor slot. |
| P1 | Loadout concept | The portrait concept combines race preview, editable board, and post-race reward cards in one view. | Keep Setup focused on board editing, inventory, track context, and one `RACE` action. Move reward cards exclusively to the post-race Reward screen. |
| P1 | Result concept | `1ST PLACE` is not supported by the current result model. The concept also omits the implemented `Continue` route. | Use implemented truth such as `TIER 1`, `NEW BEST`, time, score, and gold. Make `CHOOSE UPGRADE` primary and `CONTINUE` a secondary text action. |
| P1 | Ranking concept | The five-car leaderboard invents players, identity, sorting, ties, freshness, and online behavior. Current data supports only one best time per track and tier thresholds. | For the current product, show `BEST TIME` and tier thresholds. Keep social ranking as roadmap until a leaderboard service, identity model, offline cache, tie rules, and integrity policy exist. |
| P1 | Maps | No current campaign API selects a track or exposes regions, locks, nodes, or map progress. | Define `TrackSelection`, `RegionProgress`, `UnlockState`, and persistence contracts before production art or UI implementation. |
| P1 | Navigation depth | World Map → Local Map adds a full transition before Setup in a short-session game. | Start with one `Tour` screen: regions remain visible while the selected region expands to reveal tracks. Split into two screens only after region content becomes dense. |
| P1 | Action clarity | Several orange buttons are visually prominent but unlabeled. | Use explicit task verbs: `RACE`, `VIEW TRACK`, `SELECT REGION`, `BACK`, or `CONTINUE`. Never rely on button color or position to convey meaning. |
| P1 | Selection | Rewards and maps rely heavily on orange borders, scale, or color. | Add a checkmark and `SELECTED` label, plus a stable border. Selection must survive desaturation and low-vision viewing. |
| P1 | Recovery | Loading, empty, offline, stale, disabled, and error states are absent from the concepts. | Treat each state as a designed screen state, not a toast-only exception. Preserve user input and provide one clear recovery action. |
| P1 | Back behavior | Ranking and map concepts do not define a persistent return path. | Use one consistent top-level back affordance and platform back handling. Preserve track selection, board layout, scroll position, and reward roll. |
| P2 | Reward comprehension | Reward cards show category names and icons but not the gameplay delta. | Add one concise effect line, for example `+12% grip for 2 laps`, and reflect the pending change before confirmation. |
| P2 | Visual system | Card radii, title scale, outline weight, shadow depth, and icon rendering vary between screens. | Define a small token sheet for spacing, corner radius, strokes, elevation, typography, and semantic colors. Use the same component geometry across screens. |
| P2 | Production text | The concepts use baked raster text, suitable for press material but not production UI. | Rebuild labels as live TextMeshPro content with localization, dynamic sizing, accessibility names, and contrast verification. |

## Screen-by-screen direction

### 1. Product vision — landscape

**Keep:** the immediate cause-and-effect story between the gear board and the car, the strong car silhouette, the track readability, and the explicit concept-art disclosure.

**Change for future press variants:** keep this as explanatory marketing art, not a literal UI frame. Avoid reintroducing arrows or connector lines; adjacency and matched accent colors communicate the relationship more naturally.

### 2. Product vision — portrait / Setup

**Keep:** the figure-eight preview, centered board, tactile gear pieces, and compact vertical composition.

**Change:** remove the three reward cards from Setup. Replace them with a collapsible inventory tray or category tabs. Label the orange action `RACE`. Add a board-validity state, selected-part details, removal/undo affordances, currency or capacity if relevant, and a route back to track context.

Recommended compact order:

```text
Track context
Board workspace
Validation or selected-part detail
Inventory tray
Sticky RACE action
```

### 3. Ranking

**Keep:** the fast scan pattern, clear title, track identity, highlighted player row, and simple time formatting.

**Change:** choose one truthful product mode. The implementation-ready mode is `Personal best`: best time, target tiers, last result, and improvement delta. A social leaderboard requires named opponents, player identity, rank movement, tie handling, freshness, offline/stale messaging, and a defined data source. Ranking should open from track detail or the result screen, not interrupt the core loop.

### 4. Reward

**Keep:** three-card comparison, strong visual differentiation, selected-card elevation, and the relationship between rewards and the gear system.

**Change:** add effect text, rarity/category, eligibility, selected state, and one confirmation action. Keep `REROLL` and `SKIP` secondary. Design explicit states for loading, ad unavailable, ad cancelled, inventory full, and selection failure. The current roll must remain stable during recovery.

### 5. Match result

**Keep:** celebratory hierarchy, large result value, reward summary, and clear upgrade momentum.

**Change:** replace unsupported placement with implemented data. A recommended hierarchy is:

```text
RACE COMPLETE
NEW BEST / TIER ACHIEVED
00:23.81
Score · Gold · Laps
CHOOSE UPGRADE
Continue without upgrade
```

Show the result immediately from local simulation data. A small sync status can update independently. Celebration should not delay access to the actions.

### 6. World map

**Keep:** the diorama language, strong region silhouettes, and the fantasy of a broader tour.

**Change:** treat this as roadmap art until progress and selection contracts exist. Production UI needs current region, completion, unlock reason, next objective, reward preview, selection state, and offline behavior. The bottom action must have a label.

### 7. Local map

**Keep:** visually distinct track silhouettes and a clear selected circuit.

**Change:** merge it into the Tour screen for the first implementation. When selected, a track card should expose name, lap count, target times, best time, rewards, lock state, and `SET UP RACE`. If it remains a separate view, it needs a clear back path and should preserve the chosen region.

## Target flow

### Recommended product model

1. **Tour/Main** shows current progression and the next playable track. Future regions can be visible without forcing another screen.
2. **Track selection** expands in place. Locked tracks explain the requirement; an unlocked track exposes targets, best time, and `SET UP RACE`.
3. **Setup** contains the board, inventory, track context, validation, and `RACE`. It never contains post-race reward cards.
4. **Active race** keeps the board read-only, displays essential race telemetry, and provides pause/restart behavior.
5. **Result** opens immediately from local data. Server synchronization is status, not a gate. `CHOOSE UPGRADE` is primary; `CONTINUE` is secondary.
6. **Reward** presents three explainable choices, reroll/skip alternatives, and visible recovery for inventory or ad failures.
7. **Return** restores the Tour/Main context with updated best time, currency, reward, and progression. Ranking remains an optional detail.

### Navigation ownership

The project already uses MVVM, observable properties, a navigation stack, and dependency injection. The weak point is ownership: transitions are distributed across ViewModels while `ToolbarController` also finds and activates views directly and mutates item configuration.

A single typed campaign coordinator or explicit campaign state machine should own transitions and restoration:

```text
Tour → Setup → Racing → Result → Reward → Tour
                  ↘ recoverable sync state ↗
```

Views should render state and emit intent. They should not discover sibling views or redefine navigation context. This removes hidden state changes, makes back behavior deterministic, and provides one place to preserve selected track, board layout, reward roll, and pending result synchronization.

## Adaptive layout model

Use the available window, not the physical device, as the layout input. Android's current guidance defines compact width below 600 dp, medium from 600 to 839 dp, and expanded from 840 to 1199 dp; the exact breakpoints should be validated against Gear Engine's content rather than copied as fixed device classes.

| Class | Layout behavior | Collapse order |
|---|---|---|
| Compact `<600 dp` | One task area at a time; sticky primary action; full-screen item detail or inventory sheet; no page-level horizontal scrolling. | Decoration → secondary stats → contextual help; never hide the primary task or recovery action. |
| Medium `600–839 dp` | Two panes only when height supports them: board + inventory or map + selected-track detail. | Collapse preview art before controls; stack panes when height is constrained. |
| Expanded `≥840 dp` | Stable two-pane list/detail or map/detail structure; maintain the same reading and focus order as compact. | Reduce empty decorative space before changing task order. |
| Compact height `<480 dp` | Prioritize workspace and sticky action; use drawers or sheets for secondary content. | Remove decorative header height and nonessential celebration first. |

The press-kit disclosure footer is not production UI. Removing it creates additional space, but production layouts still need explicit safe-area insets, keyboard/controller focus, and device-level validation.

## Interaction and state matrix

| Context | Empty / unavailable | Loading | Success | Error | Recovery | State to preserve |
|---|---|---|---|---|---|---|
| Tour/Main | No catalog or no unlocked track | Loading progression | Current objective and tracks | Offline or invalid catalog | Cached data + retry | Region, selected track, scroll |
| Setup | No motor or no compatible part | Restoring board | Valid board | Save/start failure | Focus invalid slot or retry | Board layout, inventory filter |
| Race | Session unavailable | Preparing simulation | Running / completed | Simulation failure | Restart or return to Setup | Track, board, local outcome |
| Result | No previous result | Syncing result | Local result + confirmed sync | Upload failure | Continue, retry in background | Full local result and pending sync |
| Reward | No valid reward pool | Loading roll | Choices available | Inventory full, ad failure, network failure | Open Storage, skip, or retry | Exact roll and selection |
| Ranking | No personal record | Loading times | Personal best or leaderboard | Offline / stale data | Cached state + retry | Track, filter, player row |
| Map | No region data | Loading progression | Selectable and locked nodes | Invalid progression | Cached tour + retry | Region, node selection, zoom |

## Accessibility and input requirements

- Use visible, explicit labels for every action. The accessible name should include the visible label.
- Use at least two selection cues, such as border plus checkmark or label; never color alone.
- Target at least 44 × 44 pt on iOS. Android production controls should use the platform's 48 dp convention. WCAG 2.2 provides a 24 × 24 CSS pixel minimum baseline for web targets, with spacing exceptions, and 44 × 44 CSS pixels at the enhanced level.
- Verify text contrast at 4.5:1 for normal text and 3:1 for large text. Verify meaningful controls and state boundaries at 3:1 against adjacent colors.
- Provide visible keyboard focus for WebGL and controller focus for console-style input. Focus should not be obscured by sticky actions or overlays.
- Do not require drag as the only way to build the board. Provide tap/select and remove or replace alternatives.
- Make result celebrations and map motion respect reduced-motion preferences. Never make motion the only carrier of state.
- Test localization expansion, text scaling, VoiceOver/TalkBack order, screen orientation policy, and safe areas on actual target devices.

## Measurable acceptance criteria

1. Every screen has one dominant action and every interactive control has a visible task label or a universally understood icon with an accessible name.
2. The path `finish race → result → reward or continue → Tour/Main` has no dead end under result-upload failure, full inventory, ad cancellation, or temporary network loss.
3. A result screen becomes actionable from local data without waiting for remote persistence. Remote status is shown separately and can retry.
4. Setup cannot silently reject `RACE`; invalid state is explained next to the board and the relevant slot receives focus.
5. Reward selection preserves the exact choices and pending selection while the player opens storage or recovers from an ad/network error.
6. Selection is understandable in grayscale and uses at least two visual cues.
7. No required page-level horizontal scrolling occurs at compact width. Compact, medium, expanded, and compact-height layouts are reviewed independently.
8. Touch targets meet the chosen platform baseline; measurements are verified in the built interface, not inferred from the concept PNGs.
9. Normal text meets 4.5:1 contrast; large text and meaningful non-text controls meet 3:1.
10. WebGL keyboard focus is visible on every interactive element, follows visual order, and is never hidden behind an overlay.
11. Navigation away and back preserves selected track, board layout, inventory filters, map position, result, and reward roll where applicable.
12. All production labels are live localized text, not baked into raster artwork.
13. If social ranking ships, its specification defines identity, privacy, sorting, ties, stale/offline state, cache lifetime, integrity, and failure recovery before UI implementation.
14. If maps ship, the domain layer exposes explicit region, node, unlock, selection, and persistence contracts before the visual concepts are converted into prefabs.

## Pattern review

| Pattern | Current evidence | Assessment |
|---|---|---|
| MVVM | Campaign Views and ViewModels | Appropriate separation for screen presentation. |
| Observer | Observable properties and view bindings | Appropriate for UI state, but error and recovery state must become observable too. |
| Dependency injection | VContainer-managed services | Appropriate; keep flow coordination injected and testable. |
| Navigation stack | ViewModels open and close campaign screens | Useful foundation, but transition ownership is distributed. |
| State / coordinator | No single explicit campaign flow owner | Recommended to make transitions, back behavior, restoration, and error recovery deterministic. |
| Service boundary | Track and reward services expose persistence/game data | Needs selection, progress-map, pending-sync, and player-facing failure contracts before roadmap screens ship. |
| Direct view discovery | Toolbar uses `FindObjectOfType` and toggles views directly | Anti-pattern: bypasses navigation ownership and can create hidden UI state. Replace with typed navigation intent through the campaign coordinator. |

## Implemented changes and residual risks

This task is an audit only. No gameplay code, prefabs, or concept images were modified. The seven visuals remain correctly disclosed as concept art, not final gameplay.

Residual risks:

- No interactive prototype was available, so gesture, keyboard/controller focus, device safe areas, text scaling, and transition timing remain unverified.
- Ranking and both maps are presentation concepts without production domain contracts.
- Raster concepts cannot establish actual target size, contrast, localization, or responsive behavior.
- The recommended coordinator/state architecture has not been implemented or regression-tested.
- Medium and expanded layouts need dedicated mocks; scaling the portrait concept is insufficient.

## Primary guidance

- [Apple Human Interface Guidelines: Designing for iOS](https://developer.apple.com/design/human-interface-guidelines/designing-for-ios)
- [Apple Design Tips](https://developer.apple.com/design/tips/)
- [Android adaptive layouts: window size classes](https://developer.android.com/develop/ui/compose/layouts/adaptive/use-window-size-classes)
- [WCAG 2.2](https://www.w3.org/TR/WCAG22/)
- [WCAG 2.2 Understanding Target Size (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html)
- [WCAG 2.2 Understanding Reflow](https://www.w3.org/WAI/WCAG22/Understanding/reflow.html)
- [WCAG 2.2 Understanding Focus Appearance](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance)
