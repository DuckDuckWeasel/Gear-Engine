# GearBoard — Reusable Standalone Board Component

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`,
`Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

Repository planning rules live at `PLANS.md`. This document must be maintained in
accordance with `PLANS.md`.

---

## Purpose / Big Picture

`BoardView` and `BoardViewModel` work, but have the wrong things inside them:

- `BoardViewModel.Initialize()` takes 7 parameters, two of which (`GearViewFactory`,
  `GearInventoryViewModel`) are view-layer concerns that the ViewModel should never own.
- `BoardViewModel` calls `viewFactory.CreateView / GetView / UnregisterView` directly,
  meaning the ViewModel is the one deciding when to spawn or destroy GameObjects.
- `BoardViewModel` subscribes to `GearDroppedFromUIEvent` internally, coupling the board
  permanently to the inventory feature.
- `BoardView` inherits `ViewComponent<BoardViewModel>`, which forces it to live inside a
  `View<GearEngineViewModel>` hierarchy. It cannot be dropped onto any other screen.
- `GearViewFactory` is a DI singleton — two boards on screen simultaneously would corrupt
  the same registry.

This plan makes **subtractive, surgical changes** to the existing files. New types are kept
to the minimum needed to remove bootstrap from the core board flow: a single startup-state
payload and its nested placement data, plus the extracted `GearBoardDragHandler`. No new
interfaces or speculative controller layers are introduced. The goal is to remove the
wrong things and move responsibility to the right layer.

**After this plan:**

- `BoardViewModel.Initialize()` takes 4 parameters. It fires C# events instead of
  touching views directly. It does not know inventory exists.
- `BoardView` is a plain `MonoBehaviour` with `Bind(BoardViewModel, interactable)` /
  `Unbind()`. It owns a local `GearViewFactory` instance. It subscribes to ViewModel
  events to spawn and despawn `GearView` children. It exposes `OnGearDroppedOverUI`.
  On bind, it pulls the current node collection from `BoardViewModel` and creates the
  initial visuals after subscriptions are in place.
- `GearBoardDragHandler` is an implementation detail of `BoardView` — wired as a private
  `[SerializeField]`. It is `internal sealed`. It only talks to `BoardView`; no external
  consumer ever holds or references it. `BoardView` enables or disables it based on the
  `interactable` flag passed to `Bind()`.
- `BoardViewModel` owns initial board-state hydration through `LoadLayout(...)`, and
  `GearInventoryViewModel` owns initial inventory hydration through `LoadInventory(...)`.
- `GearEngineViewModel` receives a `GearEngineStartData` payload, initializes child
  viewmodels, and forwards the relevant startup state to board and inventory.
- `GearBootstrap` becomes optional. In test scenes it can be a thin launcher that holds
  sample startup data and opens navigation. In production flows it can be bypassed
  entirely by whatever system provides the real board state.
- `GearViewFactory` is removed from DI. It is instantiated locally by `BoardView`, no
  longer depends on `BoardConfigSO`, and only creates/registers `GearView` hosts.
- Inventory bridging (the inventory-slot drop and the "dragged board gear over UI →
  return to inventory" path) lives in `GearEngineView.OnBind()`, not in the board.
  `GearInventorySlotView` notifies its ViewModel via `NotifyGearDropped()`; the ViewModel
  exposes a C# event that the screen subscribes to. No bus events, no injected services.
- No `[Inject]` in any view class. `GearEngineView` has zero injected fields. Views get
  everything they need from the ViewModel tree or prefab-level wiring. `IObjectResolver`
  is removed from the view layer entirely.

**Three use cases this enables with the same board component:**

| Use case | Setup |
|---|---|
| Active gear system (interactive) | `boardView.Bind(vm, interactable: true)` — drag handler activates |
| Loadout display (read-only) | `boardView.Bind(vm)` — drag handler stays inactive |
| Background visuals (auto-running) | `boardView.Bind(vm)` — drag handler stays inactive |

**How to verify:** Regenerate `Gear_Clean.unity` via Editor menu. Enter Play Mode. Gears
appear, drag/drop works, inventory bridge works, Play/Stop works. Confirm `BoardView.Bind`
can be called from a minimal scene with no `GearEngineView` parent.

---

## Progress

- [x] M1 — Add `GetAllNodes()` to `IGridManager` / `GridManager`
- [x] M2 — Trim `BoardViewModel`: remove view factory + inventory params; add `OnGearPlaced` / `OnGearRemoved` events
- [x] M3 — Refactor `BoardView` to standalone `MonoBehaviour`; own local `GearViewFactory`; react to events
- [x] M4 — Extract `GearBoardDragHandler` from `BoardView.Update()`
- [x] M5 — Introduce startup-state payloads; remove `GearViewFactory` from DI; reduce bootstrap to an optional thin launcher
- [x] M6 — Update `GearEngineViewModel`, inventory startup loading, navigation/host wiring, and `GearEngineView`
- [x] M7 — Tests, Docs, editor tool updates for startup hydration and optional bootstrap flow
- [x] Quality gate passes (`validate-changes.cmd`)

---

## Surprises & Discoveries

- `IGridNode` already exposes `ConfigData { get; }` — no cast to `NodeBase` needed when
  enumerating nodes. No interface changes required on `IGridNode`.
- `IGridManager` has no `GetAllNodes()`. This is the one additive change needed to allow
  `BoardView` to pull pre-existing nodes after startup state has been loaded into the grid
  but before the view binds.
- The existing `DragHandler.cs` in `Presentation/UI/` is for inventory slot UI drag. It
  is unrelated to the on-board gear drag currently in `BoardView.Update()`.

---

## Decision Log

- **Decision:** Do not introduce `IGearBoardController` or any new interface.
  **Rationale:** `BoardViewModel` itself is the contract. Adding an interface for
  display-only boards is premature — if that use case arrives, a `BoardViewModel`
  instantiated with only `boardConfig` and no services is sufficient. The interface layer
  would be pure indirection with no current benefit.
  **Author:** this plan

- **Decision:** `BoardView` becomes a plain `MonoBehaviour`, not `ViewComponent<T>`.
  **Rationale:** `ViewComponent<T>` ties the lifecycle to a parent `View<TViewModel>`.
  A plain `MonoBehaviour` with `Bind(BoardViewModel)` / `Unbind()` can live anywhere —
  canvas child, world space, dedicated prefab — without a parent view chain. The cost is
  one base-class change and two new methods.
  **Author:** this plan

- **Decision:** `BoardViewModel` fires C# events (`OnGearPlaced`, `OnGearRemoved`)
  instead of calling into `GearViewFactory` directly.
  **Rationale:** The ViewModel should express _what happened_, not _how to render it_.
  The view subscribes and spawns/destroys accordingly. The events are the ViewModel's
  complete view-facing surface — two delegates, no scene references.
  **Author:** this plan

- **Decision:** `GearBoardDragHandler` is `internal sealed` and wired as a private
  `[SerializeField]` on `BoardView`. It is never referenced by any external consumer.
  **Rationale:** The drag handler is an implementation detail of the board display
  component. Exposing it as a public add-on would require every screen that uses the
  board to hold a separate reference and subscribe to its events. Instead, `BoardView`
  is the single surface: `Bind(vm, interactable: true/false)` controls whether drag is
  active, and `OnGearDroppedOverUI` on `BoardView` is the only event the screen needs.
  The handler is still its own class (single responsibility, shorter files) but it is
  invisible outside the board. It only calls internal methods on `BoardView`; it never
  touches the ViewModel or inventory directly.
  **Author:** this plan

- **Decision:** `GearBoardDragHandler` calls back into `BoardView` via internal methods
  rather than calling `BoardViewModel` directly.
  **Rationale:** Keeps the data flow one-directional: handler → view → viewmodel. The
  handler has exactly one dependency (`BoardView`). `BoardView` owns the ViewModel
  reference and all event routing. This mirrors the existing convention where a child
  component talks to its parent, not to the parent's dependencies.
  **Author:** this plan

- **Decision:** Inventory bridging moves to `GearEngineView.OnBind()`.
  **Rationale:** The board should not know inventory exists. `GearDroppedFromUIEvent`
  handling and the "dragged board gear over UI → return to inventory" path are screen-level
  concerns. Moving them to the screen view removes the only feature coupling left in the
  board.
  **Author:** this plan

- **Decision:** Add `IEnumerable<IGridNode> GetAllNodes()` to `IGridManager`.
  **Rationale:** startup hydration loads logical nodes into `IGridManager` before the board
  view binds. When `BoardView.Bind()` is called later, the board needs to pull what is
  already on the grid and spawn visuals after it has subscribed to runtime events. No other
  way to read that state exists without touching `IGridManager`.
  **Author:** this plan

- **Decision:** Do not fire initial `OnGearPlaced` events from `BoardViewModel.Initialize()`.
  `BoardView.Bind()` will subscribe first, then bind the current node collection explicitly.
  **Rationale:** `GearEngineViewModel.Initialize()` runs before `GearEngineView.OnBind()`.
  If `BoardViewModel.Initialize()` emitted initial placement events immediately, they would
  be lost before `BoardView` subscribed. Binding the collection from `BoardView.Bind()`
  gives one deterministic initialization flow and avoids event replay logic.
  **Author:** this plan

- **Decision:** `BoardView` owns a private `Dictionary<IGridNode, GearView>` for the
  currently spawned visuals.
  **Rationale:** place, snap-back, and swap reuse the same node instances while mutating
  only `node.Position`. A coord-keyed dictionary becomes stale after those operations and
  can cause duplicate visual spawns. A node-keyed dictionary keeps the logical-to-visual
  mapping stable across moves and makes removal deterministic.
  **Author:** this plan

- **Decision:** `BoardViewModel` may expose `BoardConfigSO` as a property, but all world
  position, grid snapping, and rotation-offset math stays in `BoardView`.
  **Rationale:** `BoardConfigSO` is already a dependency of `BoardViewModel`, so exposing it
  is cheaper than introducing another DTO right now. However, layout math is purely a view
  concern and should stay in the view layer. This keeps the ViewModel logical and the view
  presentation-oriented.
  **Author:** this plan

- **Decision:** `GearViewFactory` no longer depends on `BoardConfigSO`.
  **Rationale:** `GearConfigData` already provides the gear-specific visual prefab. The
  board view can compute the spawn position before creating the host object. Removing board
  config from the factory keeps it focused on object creation/registration only.
  **Author:** this plan

- **Decision:** Record a follow-up, state-driven startup flow where bootstrap is optional
  and acts only as a thin scene host.
  **Rationale:** The reusable board should not depend on `GearBootstrap` for node creation
  or view creation. Long-term, a caller should provide a single start-state object, the
  feature flow should apply that state, and `BoardView` should render the resulting logical
  nodes. A test scene may still use a bootstrapper, but only to hold sample data and open
  navigation.
  **Author:** this plan

- **Decision:** Delete `GearEngineNavigationEntry` and `IGearSceneElement`. Simplify
  `GearEngineService` to depend only on `IGridManager`.
  **Rationale:** `GearEngineNavigationEntry` does exactly one thing — `navigation.Open(new
  GearEngineViewModel())` with no payload — which the new thin bootstrap launcher replaces
  entirely. `IGearSceneElement` had a single implementor (`GearBootstrap`) and a single
  consumer (`GearEngineService.SceneElement`). That property is never called in production
  code; tests already use a `StubSceneElement`. Once bootstrap is a thin launcher, the
  interface and the property become dead code. Removing them makes `GearEngineService` a
  clean single-responsibility class: wrap `IGridManager` for Play/Stop.
  **Author:** this plan

- **Decision:** `BoardViewModel.HandleInventoryDrop` takes only `(Vector3 worldPosition,
  GearConfigData gearData)` — no `GearInventoryViewModel` parameter.
  **Rationale:** Passing `GearInventoryViewModel` into `BoardViewModel` at call time still
  creates a type-level dependency between board and inventory features. Instead, the screen
  (`GearEngineView`) performs the inventory interaction — `ConsumeSpecificGear` or
  `AddGearToInventory` — before deciding whether to call the board at all. The board only
  receives the minimal data it needs for its own grid logic. This eliminates the last
  inventory reference from the board feature boundary.
  **Author:** this plan

- **Decision:** Remove `GearInventoryLoadoutSO` from DI and from `GearMechanicsInstaller`.
  **Rationale:** Inventory loading moved to `GearEngineStartData.InventoryGears`, which is
  supplied by the host at startup. The `loadout` SO was the only reason `GearMechanicsInstaller`
  accepted a loadout parameter. Removing it simplifies the installer constructor to a
  single `boardConfig` dependency and removes one serialized field from `GearMechanicsScope`.
  **Author:** this plan

- **Decision:** No `[Inject]` in any view class. `IObjectResolver` is removed from the
  view layer entirely. Views get everything they need through the ViewModel they bind to or
  through prefab-level wiring.
  **Rationale:** Views are presentation-only. Injecting services directly into views
  creates invisible coupling that bypasses the ViewModel contract, makes views
  un-instantiable outside a DI container, and spreads responsibility. The concrete
  consequence: `GearInventorySlotView` no longer needs `EventController` injected — instead
  it calls `viewModel.NotifyGearDropped(worldPos, gearData)`, and `GearInventoryViewModel`
  owns a plain C# event `OnGearDraggedToBoard` that `GearEngineView` subscribes to.
  This eliminates `IObjectResolver` from `GearEngineViewModel`, removes `SetObjectResolver`
  from `GearInventoryView`, removes the `container.Inject` calls for dynamically-created
  slot components, and removes `[Inject] EventController` from `GearEngineView`. The net
  result is that `GearEngineView` has zero `[Inject]` fields.
  The `GearDroppedFromUIEvent` bus event is no longer raised or consumed in this flow and
  is superseded by the direct C# event on the ViewModel. It can be deleted.
  **Author:** this plan

---

## Outcomes & Retrospective

Shipped: standalone `BoardView` with local `GearViewFactory`, `GearBoardDragHandler`, `GearEngineStartData` hydration, `GearTestSceneBootstrap` replacing `GearBootstrap` / `GearEngineNavigationEntry`, inventory bridging on `GearEngineView`, removal of `IGearSceneElement` and `GearDroppedFromUIEvent`. `Game.GearEngine.csproj` / `Game.GearEngine.Tests.csproj` were updated manually so `dotnet`/analyzer builds match deleted files until Unity regenerates projects.

Deferred: none required for acceptance; existing scene YAML still references removed `GearBootstrap` until **GearEngine → Create Gear_Clean Scene** (or Step 2) is run to regenerate scenes.

---

## Context and Orientation

### Files changed by this plan

| File | Change |
|---|---|
| `Manager/IGridManager.cs` | Add `IEnumerable<IGridNode> GetAllNodes()` |
| `Manager/GridManager.cs` | Implement `GetAllNodes()` — return internal dict values |
| `GearEngineStartData.cs` | Add the minimal startup-state payload types: `GearEngineStartData`, `BoardLayoutData`, `BoardGearPlacementData` |
| `Presentation/UI/BoardViewModel.cs` | Remove `GearViewFactory` + `GearInventoryViewModel` params; add `OnGearPlaced` / `OnGearRemoved` events; add `LoadLayout(...)`; rename drop handler to `HandleInventoryDrop(Vector3, GearConfigData) → bool` (no inventory param); expose `BoardConfig`, `GetNode`, and `GetCurrentNodes()` |
| `Presentation/UI/BoardView.cs` | Change base to `MonoBehaviour`; add `Bind(vm, interactable)` / `Unbind()`; own local `GearViewFactory`; maintain private `Dictionary<IGridNode, GearView>`; react to VM events; expose `OnGearDroppedOverUI`; hold private `[SerializeField] GearBoardDragHandler dragHandler`; do all layout math |
| `Bootstrap/GearViewFactory.cs` | Remove `BoardConfigSO` constructor dependency; accept precomputed spawn position; only create/register `GearView` host objects |
| `Bootstrap/GearMechanicsInstaller.cs` | Remove `GearViewFactory` singleton; remove `IGearSceneElement` registration; remove `GearBootstrap` and `GearInventoryLoadoutSO` constructor params; constructor becomes `GearMechanicsInstaller(BoardConfigSO boardConfig)` |
| `Bootstrap/GearMechanicsScope.cs` | Remove `UseEntryPoints(GearEngineNavigationEntry)`; remove `loadout` serialized field; `bootstrap` field becomes optional |
| `GearEngineService.cs` | Remove `IGearSceneElement` constructor param and `SceneElement` property — constructor becomes `GearEngineService(IGridManager gridManager)` |
| `Presentation/UI/GearInventoryViewModel.cs` | Add `LoadInventory(...)` for startup hydration; add `ConsumeSpecificGear(GearConfigData)`; add `OnGearDraggedToBoard` C# event + `NotifyGearDropped(Vector3, GearConfigData)` method |
| `Presentation/UI/GearInventoryView.cs` | Remove `IObjectResolver container` field; remove `SetObjectResolver()`; remove `container.Inject` calls on dynamically-created slot components |
| `Presentation/UI/GearInventorySlotView.cs` | Remove `[Inject] Construct(EventController)`; remove `eventController` field; call `viewModel.NotifyGearDropped(pos, BoundGearData)` instead of raising bus event |
| `Presentation/GearEngineViewModel.cs` | Remove `[Inject] GearViewFactory`; remove `[Inject] IObjectResolver`; remove `ObjectResolver` property; accept `GearEngineStartData`; initialize board and inventory from startup payload |
| `Presentation/GearEngineView.cs` | No `[Inject]` fields; subscribe `viewModel.Inventory.OnGearDraggedToBoard` instead of `GearDroppedFromUIEvent`; call `boardView.Bind(viewModel.Board, interactable: true)`; subscribe `boardView.OnGearDroppedOverUI` |
| `Tests/Editor/BoardViewModelTests.cs` | Update to new `Initialize()` signature; add event-based tests |
| `Tests/Editor/GearEngineServiceTests.cs` | Remove `StubSceneElement`; remove null-check test for `sceneElement`; update constructor calls to 1-param `IGridManager` only |
| `Tests/Editor/GearInventoryViewModelTests.cs` | Add `LoadInventory(...)` coverage if the suite does not already exist |
| `Editor/SetupTestSceneTool.cs` | Add `GearBoardDragHandler` component to `GridBoardCollider` and wire it into `BoardView`'s private `dragHandler` field; update `GearEngineView` wiring (no `boardDragHandler` field) |
| `Docs/README.md` | Update composable scene setup section |

### New files created by this plan

| File | Purpose |
|---|---|
| `Presentation/UI/GearBoardDragHandler.cs` | All pointer/touch drag logic extracted from `BoardView.Update()` |
| `GearEngineStartData.cs` | Shared startup-state payload for board layout + inventory |
| `Bootstrap/GearTestSceneBootstrap.cs` | Thin scene launcher replacing `GearBootstrap` — holds startup data, opens navigation |

### Files deleted by this plan

| File | Reason |
|---|---|
| `Presentation/GearEngineNavigationEntry.cs` | Replaced by `GearTestSceneBootstrap.Start()` — no longer needed |
| `IGearSceneElement.cs` | Dead code once bootstrap no longer seeds the board; single consumer (`GearEngineService.SceneElement`) is removed |
| `Events/GearDroppedFromUIEvent.cs` | Superseded by `GearInventoryViewModel.OnGearDraggedToBoard` C# event — no longer raised or consumed |
| `Bootstrap/GearBootstrap.cs` | Replaced by `GearTestSceneBootstrap.cs`; old bootstrap-driven seeding flow is removed from this plan |

### New types are limited to startup-state DTOs and the extracted drag handler.

---

## Plan of Work

1. **M1 — Grid enumeration**
   Add `GetAllNodes()` to the `IGridManager` interface and implement it in `GridManager`.

2. **M2 — Trim `BoardViewModel`**
   Remove `GearViewFactory` and `GearInventoryViewModel` from `Initialize()`. Replace all
   direct `viewFactory.*` calls with two C# event fires (`OnGearPlaced`, `OnGearRemoved`).
   Remove `GearDroppedFromUIEvent` subscription. Remove `boardVisualRoot` — the view owns
   its own transform. Expose `BoardConfig`, `GetNode(coord)`, and `GetCurrentNodes()` so
   the view can pull current state and apply layout math itself. Add `LoadLayout(...)` so
   startup board state is applied by the board feature rather than by bootstrap.

3. **M3 — Standalone `BoardView`**
   Change base class from `ViewComponent<BoardViewModel>` to `MonoBehaviour`. Add
   `Bind(BoardViewModel, interactable)` / `Unbind()`. Create a local `GearViewFactory` on
   `Bind`. Subscribe to ViewModel events, then bind the current node collection and
   spawn/destroy `GearView` children accordingly. `BoardView` owns all layout math using
   `viewModel.BoardConfig`.

4. **M4 — `GearBoardDragHandler`**
   Extract everything inside `BoardView.HandleBoardDragInteraction()` (and its helpers)
   into a new `GearBoardDragHandler : MonoBehaviour`. `GearBoardDragHandler` calls only
   into `BoardView` internal helpers; it never talks to the ViewModel or inventory directly.
   `BoardView` forwards the logical calls to the ViewModel and exposes `OnGearDroppedOverUI`
   to the outside world.

5. **M5 — Startup-state payload + factory + bootstrap cleanup**
   Add `GearEngineStartData`, `BoardLayoutData`, and `BoardGearPlacementData`. Remove
   `GearViewFactory` singleton from `GearMechanicsInstaller`. Delete
   `Bootstrap/GearBootstrap.cs` and replace it with `GearTestSceneBootstrap` — a thin
   launcher that holds `GearEngineStartData` and opens navigation in `Start()`. Delete
   `GearEngineNavigationEntry`, `IGearSceneElement`, and `GearDroppedFromUIEvent`.
   Simplify `GearEngineService` to one constructor param (`IGridManager`). Remove the
   `IGearSceneElement` registration from `GearMechanicsInstaller`. Remove the
   `UseEntryPoints` entry from `GearMechanicsScope`.

6. **M6 — Screen wiring + startup hydration**
   Update `GearEngineViewModel` to accept `GearEngineStartData`, call
   `Inventory.LoadInventory(...)`, and call `Board.LoadLayout(...)`. In
   `GearEngineView.OnBind()`: call `boardView.Bind(viewModel.Board, interactable: true)` and
   wire both inventory bridges (`viewModel.Inventory.OnGearDraggedToBoard` and
   `OnGearDroppedOverUI`). Remove `IObjectResolver` from the view layer entirely.

7. **M7 — Tests, Docs, editor tools**
   Update `BoardViewModelTests` to use the new signature. Add event-based tests and startup
   hydration tests. Add verification for the new inventory-slot → ViewModel → screen bridge.
   Delete `Events/GearDroppedFromUIEvent.cs`. Update inventory tests,
   `SetupTestSceneTool`, and `README.md`.

---

## Concrete Steps

### Milestone 1 — Grid enumeration

**File: `Assets/Scripts/Game/GearEngine/Manager/IGridManager.cs`**

Add one member to the existing interface:

```csharp
IEnumerable<IGridNode> GetAllNodes();
```

**File: `Assets/Scripts/Game/GearEngine/Manager/GridManager.cs`**

Implement it. The exact internal field name is in `GridManager.cs` — it is the
`Dictionary<Vector2Int, IGridNode>` backing `GetNode()`. Add:

```csharp
public IEnumerable<IGridNode> GetAllNodes() => nodes.Values;
```

---

### Milestone 2 — Trim `BoardViewModel`

The goal is to remove the two wrong parameters and replace all direct view manipulation
with event fires.

**Signature change — `Initialize()`:**

```csharp
// Before (7 params)
public void Initialize(
    IGearEngineService engineService,
    IGridManager gridManager,
    GearNodeFactory nodeFactory,
    GearViewFactory viewFactory,          // ← remove
    GearInventoryViewModel inventory,     // ← remove
    BoardConfigSO boardConfig,
    EventController eventController)      // ← remove (only needed for GearDroppedFromUIEvent)

// After (4 params)
public void Initialize(
    IGearEngineService engineService,
    IGridManager gridManager,
    GearNodeFactory nodeFactory,
    BoardConfigSO boardConfig)
```

**Add two C# events:**

```csharp
// View subscribes to these. IGridNode is already part of the board's logical model.
public event Action<IGridNode> OnGearPlaced;
public event Action<IGridNode> OnGearRemoved;
```

**Expose helpers the view can use after it binds:**

```csharp
public BoardConfigSO BoardConfig => boardConfig;
public IGridNode GetNode(Vector2Int coord) => gridManager.GetNode(coord);
public IEnumerable<IGridNode> GetCurrentNodes() => gridManager.GetAllNodes();
```

Do **not** fire initial `OnGearPlaced` events inside `Initialize()`. `BoardView.Bind()`
subscribes first, then explicitly binds the current node collection via `GetCurrentNodes()`.

**Add startup hydration:**

```csharp
public void LoadLayout(BoardLayoutData layout)
{
    if (layout == null)
        throw new ArgumentNullException(nameof(layout));
    if (gridManager == null || nodeFactory == null || boardConfig == null)
        throw new InvalidOperationException("BoardViewModel must be initialized before LoadLayout.");

    foreach (BoardGearPlacementData placement in layout.Placements)
    {
        if (placement == null || placement.GearConfig == null)
            continue;

        Vector2Int pos = placement.Position;
        bool inBounds =
            pos.x >= 0 && pos.x < boardConfig.GridWidth &&
            pos.y >= 0 && pos.y < boardConfig.GridHeight;

        if (!inBounds)
        {
            Debug.LogError($"[BoardViewModel] Ignoring out-of-bounds starting gear at {pos}.");
            continue;
        }

        if (gridManager.GetNode(pos) != null)
        {
            Debug.LogError($"[BoardViewModel] Duplicate starting gear at {pos}.");
            continue;
        }

        GearConfigData runtimeData = placement.GearConfig.CreateRuntimeData();
        IGridNode node = nodeFactory.CreateNode(pos, runtimeData);
        gridManager.AddNode(node);
    }
}
```

`LoadLayout(...)` is for **initial hydration only**. It should stay silent and should not
emit `OnGearPlaced`. `BoardView.Bind()` already turns the current grid contents into the
initial visuals by iterating `GetCurrentNodes()`.

**Lifecycle rule for the view registry:**

`BoardView` must track visuals by **node instance**, not by coordinate. The key insight is
that place, snap-back, and swap reuse the same `IGridNode` objects and only mutate
`node.Position`. A coordinate-keyed dictionary becomes stale after those operations and can
accidentally double-spawn visuals. Therefore:

- `BoardView` stores `Dictionary<IGridNode, GearView> viewsByNode`
- `GearView.Update()` continues to follow `targetNode.Position`
- `OnGearPlaced` / `OnGearRemoved` are **view-lifecycle events only**:
  - fire `OnGearPlaced(node)` when a brand new logical node appears
  - fire `OnGearRemoved(node)` when an existing logical node disappears
- Do **not** fire placement/removal events for plain repositioning of the same node
  (`PlaceNodeAt`, `SnapNodeBackToOriginal`, `SwapBoardGears`). The existing `GearView`
  should remain alive and simply follow the moved node.

This keeps board movement cheap and avoids duplicate `GearView` creation during swaps or
snap-back.

**Replace all `viewFactory.*` calls with event fires:**

Every place `BoardViewModel` currently calls `viewFactory.CreateView(...)` for a **new**
logical node becomes `OnGearPlaced?.Invoke(node)`.

Every place it calls `viewFactory.UnregisterView(node)` + `DestroyViewGameObject(go)` for a
node that is actually leaving the board becomes `OnGearRemoved?.Invoke(node)`.

The view is responsible for the actual spawn and destroy — the ViewModel only declares
that a placement or removal occurred.

**Concrete replacements:**

| Old call in BoardViewModel | New code |
|---|---|
| `viewFactory.CreateView(newNode, data, parent)` | `OnGearPlaced?.Invoke(newNode)` |
| `DestroyGearViewForNode(node)` + view destroy | `OnGearRemoved?.Invoke(node)` |
| `viewFactory.GetView(node)?.RecalculateRotationOffset()` | Remove — the existing `GearView` follows `node.Position` and recalculates in `GearView.Update()` |

**Event matrix for each logical path:**

| BoardViewModel path | Logical result | View lifecycle events |
|---|---|---|
| `PlaceNodeAt(node, toPos)` | same node moved to empty cell | none |
| `SnapNodeBackToOriginal(node)` | same node moved back | none |
| `SwapBoardGears(draggedNode, occupantNode, targetDropPos)` | same two nodes exchanged positions | none |
| `MergeBoardGearsAt(draggedNode, occupantNode, targetDropPos, occupantData)` | two old nodes removed, one upgraded node created | `OnGearRemoved(draggedNode)`, `OnGearRemoved(occupantNode)`, `OnGearPlaced(newNode)` |
| Return board gear to inventory | one node removed from board | `OnGearRemoved(node)` |
| Inventory drop to empty cell | one new node created | `OnGearPlaced(newNode)` |
| Inventory merge onto matching cell | one old node removed, one upgraded node created | `OnGearRemoved(occupant)`, `OnGearPlaced(newNode)` |

**Remove `boardVisualRoot` entirely:**

Delete `private Transform boardVisualRoot` and `SetBoardVisualRoot(Transform root)`.
The view owns its own transform. The ViewModel never needs a scene reference.

**Remove `GearDroppedFromUIEvent` subscription:**

Delete `eventController.AddListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI)`,
the `HandleGearDroppedFromUI` method, and the `Dispose()` unsubscription. This logic
moves to `GearEngineView.OnBind()` in M6.

**`OnGearPickedUp` and `OnGearDropped` stay on `BoardViewModel`:**

These are still called by `GearBoardDragHandler` in M4. The internal logic (place, swap,
merge, snap-back) stays in the ViewModel — that is data-layer decision logic, not view
logic. The only change is that after each decision, the ViewModel fires the appropriate
events instead of directly calling the factory.

---

### Milestone 3 — Standalone `BoardView`

**File: `Assets/Scripts/Game/GearEngine/Presentation/UI/BoardView.cs`**

**Base class change:**

```csharp
// Before
public class BoardView : ViewComponent<BoardViewModel>

// After
public class BoardView : MonoBehaviour
```

**Add the drag handler private field and the `OnGearDroppedOverUI` event:**

```csharp
// Wired in Inspector by the editor tool. Null on display-only prefabs.
[SerializeField] private GearBoardDragHandler dragHandler;

// The only event external consumers (screens) subscribe to for board→inventory returns.
public event Action<GearConfigData, Vector3> OnGearDroppedOverUI;
```

**Add `Bind(vm, interactable)` / `Unbind()`:**

```csharp
private BoardViewModel viewModel;
private GearViewFactory localFactory;
private readonly Dictionary<IGridNode, GearView> viewsByNode = new();

public void Bind(BoardViewModel vm, bool interactable = false)
{
    Unbind();
    viewModel = vm ?? throw new ArgumentNullException(nameof(vm));
    localFactory = new GearViewFactory();

    vm.OnGearPlaced  += HandleGearPlaced;
    vm.OnGearRemoved += HandleGearRemoved;

    foreach (IGridNode node in vm.GetCurrentNodes())
        SpawnView(node);

    if (dragHandler != null)
        dragHandler.enabled = interactable;
}

public void Unbind()
{
    if (viewModel == null) return;
    viewModel.OnGearPlaced  -= HandleGearPlaced;
    viewModel.OnGearRemoved -= HandleGearRemoved;
    DestroyAllViews();
    localFactory = null;
    viewModel = null;

    if (dragHandler != null)
        dragHandler.enabled = false;
}
```

`BoardView` does not own a serialized `BoardConfigSO`. It reads `viewModel.BoardConfig`
whenever it needs grid snapping, world positions, or drag grab distance.

**Add internal callbacks for `GearBoardDragHandler` to call back into:**

```csharp
// Called by GearBoardDragHandler only — internal to the board package.
internal void NotifyPickedUp(IGridNode node, Vector2Int coord)
    => viewModel?.OnGearPickedUp(node, coord);

internal void NotifyDropped(IGridNode node, Vector2Int coord)
    => viewModel?.OnGearDropped(node, coord, overUI: false);

internal void NotifyDroppedOverUI(GearConfigData config, Vector3 worldPos)
    => OnGearDroppedOverUI?.Invoke(config, worldPos);

internal IEnumerable<GearView> GetViews()
    => viewsByNode.Values;

internal bool IsRunning() => viewModel?.EngineService?.IsRunning ?? false;
internal BoardConfigSO GetBoardConfig() => viewModel?.BoardConfig;
```

These `internal` methods are the complete API surface between the drag handler and the
rest of the board. No external class can see or call them.

**Event handlers that drive spawning:**

```csharp
private void HandleGearPlaced(IGridNode node)
{
    if (node == null) return;
    SpawnView(node);
}

private void HandleGearRemoved(IGridNode node)
{
    if (node == null) return;
    if (!viewsByNode.TryGetValue(node, out GearView view))
        return;

    viewsByNode.Remove(node);
    DestroyGO(view.gameObject);
}

private void SpawnView(IGridNode node)
{
    if (node == null) return;
    if (viewsByNode.ContainsKey(node)) return;

    Vector3 localPosition = viewModel.BoardConfig.GetWorldPosition(node.Position);
    GearView view = localFactory.CreateView(node, node.ConfigData, transform, localPosition);
    view.Initialize(node, node.ConfigData, viewModel.BoardConfig, localFactory);
    viewsByNode[node] = view;
}
```

**Remove `OnBind()` override — replace with `Bind()` method.**

**Remove `OnDestroy()` — replace with `Unbind()` called from `OnDestroy()`:**

```csharp
private void OnDestroy() => Unbind();
```

**Remove `Update()` and all drag helper methods** — these move to `GearBoardDragHandler`
in M4. `BoardView` becomes display-only.

**No public view enumeration or ViewModel accessor needed.**

`GearBoardDragHandler` accesses views and board state exclusively through the `internal`
callback methods added above (`GetViews()`, `IsRunning()`, `GetBoardConfig()`). Nothing is
exposed publicly for this purpose.

`BoardView` does **not** try to re-key visuals during move/swap/snap-back. The same
`GearView` instance stays attached to the same `IGridNode` instance and naturally follows
the node's updated `Position` through `GearView.Update()`.

---

### Milestone 4 — `GearBoardDragHandler`

**New file: `Assets/Scripts/Game/GearEngine/Presentation/UI/GearBoardDragHandler.cs`**

`GearBoardDragHandler` is `internal sealed` — invisible outside the `Game.GearEngine`
assembly. It has exactly one dependency: `BoardView` (its sibling component on the same
GameObject). It never touches `GearInventoryViewModel` or any event bus. All communication
goes through `BoardView`'s internal callback methods.

It starts disabled (`enabled = false`). `BoardView.Bind()` enables it only when
`interactable: true` is passed.

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GearEngine.Presentation
{
    [RequireComponent(typeof(BoardView))]
    internal sealed class GearBoardDragHandler : MonoBehaviour
    {
        private BoardView boardView;
        private Camera mainCamera;

        private GearView draggedView;
        private Vector2Int originalGridPos;

        private void Awake() => boardView = GetComponent<BoardView>();
        private void Start() => mainCamera = Camera.main;

        private void Update()
        {
            if (boardView == null || mainCamera == null) return;
            if (boardView.IsRunning()) return;

            Vector3 worldPos = GetWorldPointerPosition();

            if (IsPointerDown())  HandlePickup(worldPos);
            if (IsPointerHeld())  HandleHover(worldPos);
            if (IsPointerUp())    HandleDrop(worldPos);
        }

        private void HandlePickup(Vector3 worldPos)
        {
            BoardConfigSO boardConfig = boardView.GetBoardConfig();
            float closestDist = boardConfig.MaxDragGrabDistance;
            GearView closest = null;

            foreach (GearView view in boardView.GetViews())
            {
                if (view == null || view.TargetNode == null || !view.TargetNode.IsInteractable)
                    continue;

                float dist = Vector2.Distance(
                    new Vector2(view.transform.position.x, view.transform.position.y),
                    new Vector2(worldPos.x, worldPos.y));

                if (dist < closestDist) { closestDist = dist; closest = view; }
            }

            if (closest == null) return;

            draggedView = closest;
            draggedView.IsBeingDragged = true;
            originalGridPos = closest.TargetNode.Position;
            boardView.NotifyPickedUp(closest.TargetNode, originalGridPos);
        }

        private void HandleHover(Vector3 worldPos)
        {
            if (draggedView != null)
                draggedView.transform.position = worldPos;
        }

        private void HandleDrop(Vector3 worldPos)
        {
            if (draggedView == null) return;

            bool overUI = IsPointerOverUI();

            if (overUI)
            {
                GearConfigData droppedConfig = draggedView.TargetNode?.ConfigData;
                draggedView.IsBeingDragged = false;
                DestroyGO(draggedView.gameObject);
                draggedView = null;
                boardView.NotifyDroppedOverUI(droppedConfig, worldPos);
                return;
            }

            Vector2Int targetPos = boardView.GetBoardConfig().GetGridPosition(worldPos);
            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            draggedView.IsBeingDragged = false;
            draggedView = null;
        }

        // ── Pointer helpers ────────────────────────────────────────────────────
        private bool IsPointerDown()
            => Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        private bool IsPointerHeld()
            => Input.GetMouseButton(0)
            || (Input.touchCount > 0
                && (Input.GetTouch(0).phase == TouchPhase.Moved
                    || Input.GetTouch(0).phase == TouchPhase.Stationary));

        private bool IsPointerUp()
            => Input.GetMouseButtonUp(0)
            || (Input.touchCount > 0
                && (Input.GetTouch(0).phase == TouchPhase.Ended
                    || Input.GetTouch(0).phase == TouchPhase.Canceled));

        private Vector3 GetPointerPosition()
            => Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

        private Vector3 GetWorldPointerPosition()
        {
            Vector3 p = GetPointerPosition();
            p.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 world = mainCamera.ScreenToWorldPoint(p);
            world.z = -1f;
            return world;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return Input.touchCount > 0
                ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                : EventSystem.current.IsPointerOverGameObject();
        }

        private static void DestroyGO(GameObject go)
        {
            if (go == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying) { UnityEngine.Object.DestroyImmediate(go); return; }
#endif
            UnityEngine.Object.Destroy(go);
        }
    }
}
```

**What `GearBoardDragHandler` knows about:**
- `BoardView` (its sibling) — calls `NotifyPickedUp`, `NotifyDropped`, `NotifyDroppedOverUI`, `GetViews`, `IsRunning`, `GetBoardConfig`

**What `GearBoardDragHandler` does NOT know about:**
- `BoardViewModel` directly — zero reference
- `GearInventoryViewModel` — zero reference
- Any event bus — zero reference
- Any screen-level type — zero reference

---

### Milestone 5 — Startup-state payload + factory cleanup

**New file: `Assets/Scripts/Game/GearEngine/GearEngineStartData.cs`**

Add the minimal startup-state payload types:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        [SerializeField] private BoardLayoutData boardLayout;
        [SerializeField] private List<GearConfig> inventoryGears = new();

        public BoardLayoutData BoardLayout => boardLayout;
        public IReadOnlyList<GearConfig> InventoryGears => inventoryGears;
    }

    [Serializable]
    public sealed class BoardLayoutData
    {
        [SerializeField] private List<BoardGearPlacementData> placements = new();

        public IReadOnlyList<BoardGearPlacementData> Placements => placements;
    }

    [Serializable]
    public sealed class BoardGearPlacementData
    {
        [SerializeField] private Vector2Int position;
        [SerializeField] private GearConfig gearConfig;

        public Vector2Int Position => position;
        public GearConfig GearConfig => gearConfig;
    }
}
```

**File: `Bootstrap/GearMechanicsInstaller.cs`**

The installer constructor must be simplified to accept only `BoardConfigSO`:

```csharp
// Before (remove these params)
public GearMechanicsInstaller(BoardConfigSO boardConfig, GearBootstrap bootstrap, GearInventoryLoadoutSO loadout)

// After
public GearMechanicsInstaller(BoardConfigSO boardConfig)
```

Remove the following DI registrations:

```csharp
builder.Register<GearViewFactory>(Lifetime.Singleton);                       // removed
builder.RegisterComponent(bootstrap).As<IGearSceneElement>();                // removed
builder.RegisterInstance(loadout);                                           // removed
```

Everything else (`GearEngineService`, `IGridManager`, `GearNodeFactory`, `BoardConfigSO`) stays.

**File: `Bootstrap/GearViewFactory.cs`**

Remove the constructor dependency on `BoardConfigSO`. The factory should no longer compute
board positions or know about board layout.

```csharp
// Before
public class GearViewFactory
{
    private readonly BoardConfigSO boardConfig;

    public GearViewFactory(BoardConfigSO boardConfig)
    {
        this.boardConfig = boardConfig;
    }

    public GearView CreateView(IGridNode node, GearConfigData configData, Transform parent)
    {
        GameObject viewObj = new GameObject($"{node.GetType().Name}_{node.Position}");
        viewObj.transform.SetParent(parent);
        viewObj.transform.localPosition = boardConfig.GetWorldPosition(node.Position);
        var view = viewObj.AddComponent<GearView>();
        view.Initialize(node, configData, boardConfig, this);
        viewRegistry[node] = view;
        return view;
    }
}

// After
public class GearViewFactory
{
    public GearView CreateView(
        IGridNode node,
        GearConfigData configData,
        Transform parent,
        Vector3 localPosition)
    {
        GameObject viewObj = new GameObject($"{node.GetType().Name}_{node.Position}");
        viewObj.transform.SetParent(parent);
        viewObj.transform.localPosition = localPosition;
        var view = viewObj.AddComponent<GearView>();
        viewRegistry[node] = view;
        return view;
    }
}
```

`BoardView` computes `localPosition` using `viewModel.BoardConfig.GetWorldPosition(...)`
and then calls `view.Initialize(node, configData, viewModel.BoardConfig, localFactory)`.
`CreateView(...)` must **not** call `Initialize(...)` internally anymore. Initialization
must happen exactly once in `BoardView.SpawnView(...)`.

`GearView` still needs `BoardConfigSO` during `Initialize(...)` because its `Update()`
method lerps toward the logical board position every frame.

**File: `Bootstrap/GearBootstrap.cs`**

Delete this file. Its old board-seeding responsibility is replaced by
`GearTestSceneBootstrap.cs`.

**New file: `Bootstrap/GearTestSceneBootstrap.cs`**

Add an optional thin launcher for test scenes. It should hold startup data and open
navigation. It should **not** inject `IGridManager`, `GearNodeFactory`, or
`GearViewFactory`, and it should not create nodes or views.

```csharp
using System;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public sealed class GearTestSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GearEngineStartData startData;

        private INavigation navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        private void Start()
        {
            try
            {
                navigation.Open(new GearEngineViewModel(startData));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTestSceneBootstrap] Failed to open gear engine screen: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
```

**File: `IGearSceneElement.cs`** — Delete.

**File: `Presentation/GearEngineNavigationEntry.cs`** — Delete.

**File: `GearEngineService.cs`**

Remove the `IGearSceneElement` dependency. The service only wraps `IGridManager` for
Play/Stop:

```csharp
public sealed class GearEngineService : IGearEngineService
{
    private readonly IGridManager gridManager;

    public GearEngineService(IGridManager gridManager)
    {
        this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
    }

    public bool IsRunning => gridManager.IsRunning;
    public void Play() => gridManager.Play();
    public void Stop() => gridManager.Stop();
}
```

**File: `Bootstrap/GearMechanicsScope.cs`**

Remove the `UseEntryPoints` line — the thin bootstrap MonoBehaviour launches the screen
directly from its `Start()`:

```csharp
// Remove this line:
builder.UseEntryPoints(ep => ep.Add<GearEngineNavigationEntry>());
```

Also remove the null-check for `bootstrap` if the new `GearTestSceneBootstrap` is no
longer passed into the scope as a required field. The scope only needs `boardConfig` as a
mandatory serialized field. `loadout` is removed entirely. `bootstrap` may remain as an
optional serialized reference for test scenes that still use the thin launcher.

---

### Milestone 6 — Screen wiring + startup hydration

**File: `Presentation/GearEngineViewModel.cs`**

Remove `[Inject] private GearViewFactory viewFactory` (the board no longer needs it from
the parent VM). Replace the implicit startup state with an explicit constructor payload:

```csharp
public sealed class GearEngineViewModel : ViewModel
{
    private readonly GearEngineStartData startData;

    // No IObjectResolver — views get everything from the ViewModel tree.
    [Inject] private IGearEngineService engineService;
    [Inject] private IGridManager gridManager;
    [Inject] private GearNodeFactory nodeFactory;
    [Inject] private BoardConfigSO boardConfig;

    // No ObjectResolver property — removed entirely.
    public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
    public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
    public BoardViewModel Board { get; } = new BoardViewModel();

    public GearEngineViewModel(GearEngineStartData startData)
    {
        this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
    }

    protected override void Initialize()
    {
        base.Initialize();

        BindChildViewModel(SimControl);
        BindChildViewModel(Inventory);
        BindChildViewModel(Board);

        SimControl.Initialize(engineService);
        Inventory.Initialize(engineService);
        Inventory.LoadInventory(startData.InventoryGears);

        Board.Initialize(engineService, gridManager, nodeFactory, boardConfig);
        Board.LoadLayout(startData.BoardLayout);
    }
}
```

**File: `Presentation/UI/GearInventoryViewModel.cs`**

Add a batch startup API:

```csharp
public void LoadInventory(IEnumerable<GearConfig> gearConfigs)
{
    if (gearConfigs == null)
        throw new ArgumentNullException(nameof(gearConfigs));

    foreach (GearConfig config in gearConfigs)
    {
        if (config == null)
            continue;

        AddGearToInventory(config.CreateRuntimeData());
    }
}
```

This method is for startup hydration only. Keep `AddGearToInventory(...)` and
`RemoveGearFromInventory(int index)` as the existing runtime mutation APIs.

Add a plain C# event that the screen subscribes to. This replaces the `GearDroppedFromUIEvent`
bus event — no `EventController` needed in this class:

```csharp
// Raised when an inventory slot is successfully dropped on a valid board target.
// GearEngineView subscribes and coordinates the board placement + inventory removal.
public event Action<Vector3, GearConfigData> OnGearDraggedToBoard;

public void NotifyGearDropped(Vector3 worldPos, GearConfigData gearData)
{
    if (gearData == null)
        throw new ArgumentNullException(nameof(gearData));

    OnGearDraggedToBoard?.Invoke(worldPos, gearData);
}
```

Also add `ConsumeSpecificGear(GearConfigData)` — a targeted removal called by
`GearEngineView` when a board drop succeeds:

```csharp
public void ConsumeSpecificGear(GearConfigData gearData)
{
    if (gearData == null)
        throw new ArgumentNullException(nameof(gearData));

    int index = FindGearIndex(gearData);
    if (index < 0)
    {
        Debug.LogError($"[GearInventoryViewModel] ConsumeSpecificGear: gear not found in inventory.");
        return;
    }

    RemoveGearFromInventory(index);
}
```

`FindGearIndex` is an existing or new private helper that matches by config identity.

**File: `Presentation/UI/GearInventorySlotView.cs`**

Remove the `[Inject]` method entirely. The slot calls back to its ViewModel instead of
raising a bus event:

```csharp
// Remove entirely:
// [Inject]
// public void Construct(EventController eventController) { ... }

private void HandleValidDrop(Vector3 targetWorldPosition)
{
    if (BoundGearData == null) return;

    // Notify through the ViewModel — no EventController needed in the view.
    viewModel.NotifyGearDropped(targetWorldPosition, BoundGearData);
}
```

`DragHandler` also had `container.Inject(dragger)` called on it from `GearInventoryView`.
`DragHandler` has no `[Inject]` attributes, so that call was always a no-op — remove it.

**File: `Presentation/UI/GearInventoryView.cs`**

Remove `IObjectResolver`, `SetObjectResolver`, and both `container.Inject` calls:

```csharp
// Remove:
// private IObjectResolver container;
// public void SetObjectResolver(IObjectResolver resolver) { container = resolver; }

// In RebuildUIList(), remove:
// if (container != null) { container.Inject(slotView); container.Inject(dragger); }
```

`GearInventoryView` is now a pure view: it holds a ViewModel reference (via `Bind`),
subscribes to collection changes, and builds slot GameObjects with no DI involvement.

**File: `Presentation/GearEngineView.cs`**

`GearEngineView` has **zero `[Inject]` fields**. Everything comes from the ViewModel tree
or prefab-level `[SerializeField]` wiring. `GearBoardDragHandler` is invisible here.

```csharp
// No [Inject] fields.
[SerializeField] private SimulationControlView simControlView;
[SerializeField] private GearInventoryView inventoryView;
[SerializeField] private BoardView boardView;   // ← the only board reference
// No boardDragHandler field. The drag handler is BoardView's internal concern.
```

Wire everything in `OnBind()`:

```csharp
protected override void OnBind()
{
    // No SetObjectResolver — IObjectResolver is removed from the view layer.
    simControlView.Bind(viewModel.SimControl);
    inventoryView.Bind(viewModel.Inventory);
    boardView.Bind(viewModel.Board, interactable: true);

    // Bridge 1: board gear dragged over UI → return to inventory.
    boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;

    // Bridge 2: inventory slot dropped on board → place on board, consume from inventory.
    // OnGearDraggedToBoard is a plain C# event on GearInventoryViewModel.
    // No EventController or bus events needed.
    viewModel.Inventory.OnGearDraggedToBoard += HandleGearDraggedToBoard;
}

private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
{
    viewModel.Inventory.AddGearToInventory(config);
}

private void HandleGearDraggedToBoard(Vector3 worldPos, GearConfigData gearData)
{
    try
    {
        if (viewModel.Board.EngineService.IsRunning) return;

        // Screen coordinates the two-step transaction: board decides placement,
        // screen removes from inventory on success.
        bool placed = viewModel.Board.HandleInventoryDrop(worldPos, gearData);
        if (placed)
            viewModel.Inventory.ConsumeSpecificGear(gearData);
    }
    catch (Exception ex)
    {
        Debug.LogError($"[GearEngineView] HandleGearDraggedToBoard failed: {ex.Message}\n{ex.StackTrace}");
    }
}

private void OnDestroy()
{
    boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
    viewModel?.Inventory.OnGearDraggedToBoard -= HandleGearDraggedToBoard;
    boardView?.Unbind();
}
```

**Note on `HandleInventoryDrop`:**

The grid-placement decision logic stays in `BoardViewModel`. Rename the existing method to
`HandleInventoryDrop` with the signature:

```csharp
// Returns true if the gear was successfully placed or merged onto the board.
public bool HandleInventoryDrop(Vector3 worldPosition, GearConfigData gearData)
```

The `GearInventoryViewModel` parameter is removed entirely. `BoardViewModel` has no
`GearInventoryViewModel` dependency — not stored, not passed at call time.

`GearEngineView` performs the inventory side of the transaction after the board reports
success: it calls `viewModel.Inventory.ConsumeSpecificGear(ctx.GearData)`. This keeps the
inventory feature boundary clean: only the screen coordinates between the two ViewModels.

`ConsumeSpecificGear(GearConfigData)` must be added to `GearInventoryViewModel` as a new
targeted removal API (complementary to the existing `RemoveGearFromInventory(int index)`).

Although the broader layout rule is "world and presentation math lives in the view", this
method intentionally keeps `worldPosition -> grid coordinate` conversion in
`BoardViewModel` via `BoardConfigSO.GetGridPosition(...)`. Treat that conversion as part of
the board-drop decision path, not as a reusable presentation API.

**`GearEngineNavigationEntry.cs` is deleted in M5** — no further changes needed here.

---

### Milestone 7 — Tests, Docs, editor tools

#### Tests

**Update `Tests/Editor/BoardViewModelTests.cs`:**

Change `Board.Initialize()` calls to the new 4-param signature. Existing behaviour tests
(`OnGearPickedUp_ExtractsNodeFromGrid`, swap, merge, snap-back, out-of-bounds) still test
the same logic, but assertions must follow the event matrix above:

- swap / snap-back / empty-cell place assert that no lifecycle event fires
- merge asserts the remove/remove/place sequence
- inventory-return asserts a remove event
- inventory-drop asserts the expected place/remove lifecycle events

Add the following new tests:

- `GetCurrentNodes_ReturnsExistingGridNodes` — seed a mock `IGridManager.GetAllNodes()`
  with two nodes; call `Initialize()`; assert `GetCurrentNodes()` yields both nodes.
- `PlaceNode_FiresOnGearPlaced` — call a path that results in a brand new node being added;
  assert the event fires with that node instance.
- `RemoveNode_FiresOnGearRemoved` — assert the event fires with the removed node instance.
- `SwapNode_DoesNotFireLifecycleEvents` — move two existing nodes and assert that no
  placement/removal event fires.
- `SnapBack_DoesNotFireLifecycleEvents` — out-of-bounds drop returns the same node and
  emits no spawn/destroy event.
- `BoardView_Bind_SpawnsViewsForExistingNodes` — if practical as an EditMode test,
  create a `BoardView`, bind a ViewModel whose grid already contains nodes, and assert
  the expected number of `GearView` children are created.
- `LoadLayout_AddsNodesToGridWithoutLifecycleEvents` — hydrate from `BoardLayoutData`,
  assert nodes are added and no startup spawn/destroy lifecycle event fires.
- `LoadLayout_RejectsDuplicatePlacements` — duplicate coordinates are ignored/logged.
- `LoadLayout_RejectsOutOfBoundsPlacements` — invalid positions are ignored/logged.
- `HandleInventoryDrop_ReturnsTrueAndPlacesGear` — valid drop returns `true` and adds node.
- `HandleInventoryDrop_ReturnsFalseForFullCell` — when target is occupied and no merge, returns `false`.
- `HandleInventoryDrop_NoInventoryParamRequired` — confirm signature takes only `(Vector3, GearConfigData)`.

**Update `Tests/Editor/GearInventoryViewModelTests.cs`** (or create the suite if absent):

- `LoadInventory_AddsAllStartingGears` — batch input adds all valid gears.
- `LoadInventory_IgnoresNullEntries` — null configs are skipped safely.
- `NotifyGearDropped_RaisesOnGearDraggedToBoard` — valid slot drop raises the plain C# event
  with the expected world position and gear.
- `ConsumeSpecificGear_RemovesMatchingGear` — a known gear is removed by config identity.
- `ConsumeSpecificGear_LogsErrorWhenGearNotFound` — missing gear logs error, no throw.

**Update `Tests/Editor/GearEngineViewTests.cs`** (or add focused coverage in an existing
view suite if one already exists):

- `HandleGearDraggedToBoard_OnSuccessfulPlacement_ConsumesInventory` — the screen listens to
  `OnGearDraggedToBoard`, places through `BoardViewModel`, and removes from inventory only on
  success.

#### Docs

**Update `Docs/README.md`** — composable scene setup section:

- Note that `BoardView` is now a standalone `MonoBehaviour`; `GearBoardDragHandler` is
  optional. Show the minimal setup for each use case.
- Remove mention of `GearViewFactory` as a registered singleton.
- Document that inventory bridging lives in the screen view, not the board.
- Document that views do not use `[Inject]`; view-to-service communication flows through
  ViewModels or prefab wiring only.
- Document that startup board state comes from `GearEngineStartData` / `BoardLayoutData`,
  not from bootstrap-side factory logic.

#### Editor tools

**Update `Editor/SetupTestSceneTool.cs`:**

- After `floorGrid.AddComponent<BoardView>()`, also add
  `floorGrid.AddComponent<GearBoardDragHandler>()`.
- Wire the private `dragHandler` field on `BoardView` using `SerializedObject` — same
  pattern used for all other `[SerializeField]` wiring in the tool.
- Update `GearEngineView` wiring: only the `boardView` field — no `boardDragHandler` field
  exists on `GearEngineView`.
- If the test scene keeps a bootstrap component, have it serialize a `GearEngineStartData`
  payload and open the screen through navigation rather than spawning nodes/views directly.

#### Cleanup

- Delete `Assets/Scripts/Game/GearEngine/Events/GearDroppedFromUIEvent.cs`.
- Search for any remaining references to `GearDroppedFromUIEvent` and remove them before
  running the validation gate.

---

## Validation and Acceptance

1. Run `.agents/scripts/validate-changes.cmd` — gate must be clean.
2. All tests in `Game.GearEngine.Tests` pass.
3. Open `Gear_Clean.unity` in Play Mode:
   - Gears appear at startup from the provided `GearEngineStartData` / `BoardLayoutData`
     payload and are rendered when `BoardView.Bind()` pulls `GetCurrentNodes()`.
   - Inventory appears at startup from `LoadInventory(...)`.
   - Board drag/drop/swap/merge all work.
   - Dragging a board gear over inventory UI returns it to inventory.
   - Dragging an inventory slot onto the board places the gear.
   - Play/Stop works; drag is blocked while running.
4. Confirm that `BoardView.Bind(boardViewModel, interactable: false)` can be called from a `MonoBehaviour` in
   a minimal scene with no `GearEngineView` parent — board renders, no errors.
5. No Roslyn analyzer warnings.

---

## Idempotence and Recovery

- Each milestone is independently committable.
- M1 is purely additive. No existing callers break.
- After M5, any code that `[Inject]`s `GearViewFactory` will fail at container build time
  with a clear VContainer error. Search for `[Inject].*GearViewFactory` before committing.
- After M5, any code that resolves `IGearSceneElement` from the container will fail at
  build time. Search for `IGearSceneElement` before committing M5.
- After M5, `GearEngineServiceTests` must be updated — the 2-param constructor no longer
  exists and `StubSceneElement` will be a compile error.
- `BoardView` and `BoardViewModel` files are modified in place — no renames, no deletes.

---

## Artifacts and Notes

- `Plans/GearBoardComponent/GearBoardComponent-ExecPlan.md` — this file
- Builds on top of `Plans/GearEngineRefactor/GearEngineRefactor-ExecPlan.md`
  (milestones M1–M5 and M7 of that plan must be complete before starting this one).

---

## Interfaces and Dependencies

### Before (current state)

```
BoardViewModel.Initialize(
    IGearEngineService,   // ✓ correct
    IGridManager,         // ✓ correct
    GearNodeFactory,      // ✓ correct
    GearViewFactory,      // ✗ view concern
    GearInventoryViewModel, // ✗ feature coupling
    BoardConfigSO,        // ✓ correct
    EventController)      // ✗ only needed for inventory event

BoardView : ViewComponent<BoardViewModel>   ← cannot exist without parent View<T>
  Update(): full pointer state machine mixed with display

GearViewFactory : Lifetime.Singleton       ← shared registry, breaks with two boards

GearBootstrap → viewFactory.CreateView()   ← bootstrap creates views, wrong layer
GearEngineNavigationEntry → navigation.Open(new GearEngineViewModel())  ← no startup payload
IGearSceneElement / GearEngineService.SceneElement  ← used nowhere in production
```

### After (this plan)

```
BoardViewModel.Initialize(
    IGearEngineService,   // ✓
    IGridManager,         // ✓
    GearNodeFactory,      // ✓
    BoardConfigSO)        // ✓
  + BoardConfig property   // ← view reads config, but does layout math itself
  + OnGearPlaced event     // ← only for brand new logical nodes
  + OnGearRemoved event    // ← only for nodes leaving the board
  + GetNode(coord)         // ← thin helper, delegates to gridManager
  + GetCurrentNodes()      // ← one-time bind of current collection
  + LoadLayout(layout)     // ← startup board hydration lives in the feature, not bootstrap

GearInventoryViewModel
  + LoadInventory(startingGears)  // ← startup inventory hydration

GearEngineViewModel(startData)
  + Inventory.LoadInventory(startData.InventoryGears)
  + Board.Initialize(...)
  + Board.LoadLayout(startData.BoardLayout)

BoardView : MonoBehaviour                   ← standalone; any scene, any prefab
  Bind(BoardViewModel, interactable) / Unbind()
  public event OnGearDroppedOverUI           ← only public surface screens subscribe to
  [SerializeField] private GearBoardDragHandler dragHandler  ← wired in prefab, invisible externally
  private localFactory = new GearViewFactory()               ← local, not shared
  private viewsByNode dictionary                             ← stable runtime lookup for visuals
  Reacts to: OnGearPlaced(node)  → spawn GearView if node is new
             OnGearRemoved(node) → destroy GearView for that node
  Does not respawn on place/swap/snap-back of existing nodes
  Does: all world/grid layout math using viewModel.BoardConfig
  internal: NotifyPickedUp, NotifyDropped, NotifyDroppedOverUI, GetViews, IsRunning, GetBoardConfig

GearBoardDragHandler : internal sealed MonoBehaviour   ← invisible outside assembly
  [RequireComponent(typeof(BoardView))]
  Only dependency: BoardView (sibling component)
  Calls: boardView.NotifyPickedUp / NotifyDropped / NotifyDroppedOverUI / GetViews / IsRunning / GetBoardConfig
  Enabled/disabled by BoardView.Bind(interactable)

GearEngineView.OnBind()
  boardView.Bind(viewModel.Board, interactable: true)
  boardView.OnGearDroppedOverUI → inventory.AddGearToInventory
  viewModel.Inventory.OnGearDraggedToBoard → HandleGearDraggedToBoard(...)
    → viewModel.Board.HandleInventoryDrop(...)
    → viewModel.Inventory.ConsumeSpecificGear(...)
  // No boardDragHandler reference on GearEngineView at all

GearViewFactory : plain local helper
  No BoardConfigSO dependency
  Accepts precomputed localPosition from BoardView
  Creates/registers GearView host objects only
  Does not call GearView.Initialize() internally

GearEngineService(IGridManager gridManager)   ← single dependency, no IGearSceneElement
  IsRunning / Play() / Stop() only

GearTestSceneBootstrap : MonoBehaviour
  [SerializeField] GearEngineStartData startData
  [Inject] INavigation navigation
  Start() → navigation.Open(new GearEngineViewModel(startData))
  No IGridManager / GearNodeFactory / GearViewFactory

IGearSceneElement  ← deleted
GearEngineNavigationEntry  ← deleted

GearMechanicsInstaller(BoardConfigSO boardConfig) → no GearViewFactory, no IGearSceneElement, no loadout
GearMechanicsScope → no UseEntryPoints(GearEngineNavigationEntry), no loadout field

Entry points:
  1. Host acquires GearEngineStartData (test-scene bootstrap or real game flow)
  2. Host opens GearEngineViewModel(startData)
  3. GearEngineViewModel.Initialize() hydrates inventory and board state
  4. Screen open:   GearEngineView.OnBind() → boardView.Bind(viewModel.Board, interactable: true)
                    → BoardView subscribes to runtime events
                    → BoardView iterates viewModel.GetCurrentNodes()
                    → BoardView computes positions via viewModel.BoardConfig
                    → BoardView spawns initial GearViews
  5. Standalone:    anyMonoBehaviour → boardView.Bind(someViewModel, interactable: false) — no parent required
```

## Dependency Graph

```
GearEngineView  ← no [Inject] fields
  ├── SimulationControlView
  ├── GearInventoryView       ← no IObjectResolver
  │     └── GearInventorySlotView (dynamic)  ← no [Inject]
  └── BoardView
        ├── BoardViewModel
        │     ├── IGearEngineService
        │     ├── IGridManager
        │     ├── GearNodeFactory
        │     └── BoardConfigSO
        ├── GearViewFactory (local instance, created in Bind)
        ├── Dictionary<IGridNode, GearView> (local runtime registry)
        └── GearBoardDragHandler (private serialized sibling reference)

GearEngineViewModel
  ├── [Inject] IGearEngineService
  ├── [Inject] IGridManager
  ├── [Inject] GearNodeFactory
  └── [Inject] BoardConfigSO
  (no IObjectResolver, no ObjectResolver property)

GearInventoryViewModel
  └── OnGearDraggedToBoard : event Action<Vector3, GearConfigData>
  (no EventController — event is plain C#)

GearBoardDragHandler
  └── BoardView only

GearViewFactory
  └── no DI dependencies

Optional host / bootstrap
  ├── GearEngineStartData
  └── INavigation
```

## Initialization / Usage Flow

### Initialization flow

1. `GearMechanicsScope` builds the container and registers `IGridManager`,
   `IGearEngineService`, `GearNodeFactory`, `BoardConfigSO`, and related services.
2. A host obtains a `GearEngineStartData` payload. In test scenes this can be a thin
   bootstrap component; in production this can come from whatever system owns the real
   game state.
3. The host opens `GearEngineViewModel(startData)`.
4. `GearEngineViewModel.Initialize()`:
   - initializes the child viewmodels
   - calls `Inventory.LoadInventory(startData.InventoryGears)`
   - calls `Board.Initialize(engineService, gridManager, nodeFactory, boardConfig)`
   - calls `Board.LoadLayout(startData.BoardLayout)`
5. `GearEngineView.OnBind()` — no `[Inject]` involved:
   - calls `boardView.Bind(viewModel.Board, interactable: true)`
   - subscribes `boardView.OnGearDroppedOverUI`
   - subscribes `viewModel.Inventory.OnGearDraggedToBoard`
6. `BoardView.Bind()`:
   - stores the ViewModel
   - creates a local `GearViewFactory`
   - subscribes to `OnGearPlaced` / `OnGearRemoved`
   - iterates `viewModel.GetCurrentNodes()`
   - computes each visual position with `viewModel.BoardConfig.GetWorldPosition(...)`
   - creates and initializes the `GearView` instances
   - enables `GearBoardDragHandler` if `interactable` is true

### Runtime usage flow

1. User drags on the board.
2. `GearBoardDragHandler` reads pointer state and asks `BoardView` for:
   - current views
   - board running state
   - board config for grid snapping / grab distance
3. On pickup, `GearBoardDragHandler` calls `boardView.NotifyPickedUp(...)`.
4. `BoardView` forwards the logical pickup to `BoardViewModel.OnGearPickedUp(...)`.
5. On drop, `GearBoardDragHandler` calls either:
   - `boardView.NotifyDropped(...)` for board drops
   - `boardView.NotifyDroppedOverUI(...)` for UI drops
6. `BoardView` forwards board drops to `BoardViewModel.OnGearDropped(...)`.
7. `BoardViewModel` decides the logical outcome:
   - place
   - snap back
   - swap
   - merge
   - inventory drop: `HandleInventoryDrop(worldPos, gearData) → bool`; if true, screen calls `viewModel.Inventory.ConsumeSpecificGear(gearData)`
8. `BoardViewModel` emits `OnGearPlaced` / `OnGearRemoved` only when a logical node is
   created or removed. Plain moves of an existing node emit no lifecycle event.
9. `BoardView` reacts by updating `viewsByNode`, creating or destroying `GearView`
   objects through its local `GearViewFactory`. Existing `GearView` instances keep
   following their nodes during moves, swaps, and snap-back.
10. For inventory-slot drops: `GearInventorySlotView.HandleValidDrop` calls
    `viewModel.NotifyGearDropped(worldPos, BoundGearData)` → `GearInventoryViewModel`
    raises `OnGearDraggedToBoard` → `GearEngineView.HandleGearDraggedToBoard` places the
    gear on the board and removes it from inventory. No bus events involved.
11. `GearEngineView` subscribes to `boardView.OnGearDroppedOverUI` and
    `viewModel.Inventory.OnGearDraggedToBoard`. It has no other external dependencies.

---

## Flow Graph

```mermaid
blackboard TD
    A[Host / Test Scene] --> B[GearEngineStartData]
    A --> C[GearTestSceneBootstrap]
    C --> D[INavigation.Open(new GearEngineViewModel(startData))]

    D --> E[GearEngineViewModel]
    E --> F[SimulationControlViewModel.Initialize]
    E --> G[GearInventoryViewModel.Initialize]
    E --> H[GearInventoryViewModel.LoadInventory]
    E --> I[BoardViewModel.Initialize]
    E --> J[BoardViewModel.LoadLayout]

    K[GearEngineView.OnBind] --> L[SimulationControlView.Bind]
    K --> M[GearInventoryView.Bind]
    K --> N[BoardView.Bind]
    K --> O[Subscribe boardView.OnGearDroppedOverUI]
    K --> P[Subscribe viewModel.Inventory.OnGearDraggedToBoard]

    N --> Q[Local GearViewFactory]
    N --> R[GetCurrentNodes]
    N --> S[Spawn initial GearViews]
    N --> T[Enable GearBoardDragHandler if interactable]

    U[GearBoardDragHandler] --> V[BoardView.NotifyPickedUp / NotifyDropped / NotifyDroppedOverUI]
    V --> W[BoardViewModel.OnGearPickedUp / OnGearDropped]
    W --> X{Logical result}
    X --> X1[Place]
    X --> X2[Swap]
    X --> X3[Merge]
    X --> X4[Snap back]
    X --> X5[Return over UI]

    W --> Y[OnGearPlaced / OnGearRemoved]
    Y --> Z[BoardView updates viewsByNode]

    AA[GearInventorySlotView.HandleValidDrop] --> AB[GearInventoryViewModel.NotifyGearDropped]
    AB --> AC[OnGearDraggedToBoard event]
    AC --> AD[GearEngineView.HandleGearDraggedToBoard]
    AD --> AE[BoardViewModel.HandleInventoryDrop]
    AE --> AF{Placed?}
    AF -->|Yes| AG[GearInventoryViewModel.ConsumeSpecificGear]
    AF -->|No| AH[Keep inventory unchanged]

    O --> AI[HandleGearDroppedOverUI]
    AI --> AJ[GearInventoryViewModel.AddGearToInventory]

    I --> AK[IGridManager]
    I --> AL[GearNodeFactory]
    I --> AM[BoardConfigSO]
    E --> AN[IGearEngineService]
```

## File Action Summary

| File | Action |
|---|---|
| `Manager/IGridManager.cs` | Changed |
| `Manager/GridManager.cs` | Changed |
| `GearEngineStartData.cs` | Added |
| `Presentation/UI/BoardViewModel.cs` | Changed |
| `Presentation/UI/BoardView.cs` | Changed |
| `Presentation/UI/GearBoardDragHandler.cs` | Added |
| `Bootstrap/GearViewFactory.cs` | Changed |
| `Bootstrap/GearMechanicsInstaller.cs` | Changed |
| `Bootstrap/GearMechanicsScope.cs` | Changed |
| `Bootstrap/GearTestSceneBootstrap.cs` | Added |
| `Bootstrap/GearBootstrap.cs` | Removed |
| `GearEngineService.cs` | Changed |
| `Presentation/UI/GearInventoryViewModel.cs` | Changed |
| `Presentation/UI/GearInventoryView.cs` | Changed |
| `Presentation/UI/GearInventorySlotView.cs` | Changed |
| `Presentation/GearEngineViewModel.cs` | Changed |
| `Presentation/GearEngineView.cs` | Changed |
| `Presentation/GearEngineNavigationEntry.cs` | Removed |
| `IGearSceneElement.cs` | Removed |
| `Events/GearDroppedFromUIEvent.cs` | Removed |
| `Tests/Editor/BoardViewModelTests.cs` | Changed |
| `Tests/Editor/GearEngineServiceTests.cs` | Changed |
| `Tests/Editor/GearInventoryViewModelTests.cs` | Changed |
| `Editor/SetupTestSceneTool.cs` | Changed |
| `Docs/README.md` | Changed |
