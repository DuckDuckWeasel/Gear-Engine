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

This plan makes **subtractive, surgical changes** to the existing files. No new
interfaces, no new structs, no new display-only controllers. The goal is to remove the
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
- `GearBootstrap` creates data nodes only — no view creation.
- `GearViewFactory` is removed from DI. It is instantiated locally by `BoardView`, no
  longer depends on `BoardConfigSO`, and only creates/registers `GearView` hosts.
- Inventory bridging (the `GearDroppedFromUIEvent` subscription and the "drop over UI →
  return to inventory" path) lives in `GearEngineView.OnBind()`, not in the board.

**Three use cases this enables without adding any new types:**

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

- [ ] M1 — Add `GetAllNodes()` to `IGridManager` / `GridManager`
- [ ] M2 — Trim `BoardViewModel`: remove view factory + inventory params; add `OnGearPlaced` / `OnGearRemoved` events
- [ ] M3 — Refactor `BoardView` to standalone `MonoBehaviour`; own local `GearViewFactory`; react to events
- [ ] M4 — Extract `GearBoardDragHandler` from `BoardView.Update()`
- [ ] M5 — Remove `GearViewFactory` from DI; strip view creation from `GearBootstrap`
- [ ] M6 — Wire inventory bridge in `GearEngineView`; update `GearEngineViewModel`
- [ ] M7 — Tests, Docs, editor tool updates
- [ ] Quality gate passes (`validate-changes.cmd`)

---

## Surprises & Discoveries

- `IGridNode` already exposes `ConfigData { get; }` — no cast to `NodeBase` needed when
  enumerating nodes. No interface changes required on `IGridNode`.
- `IGridManager` has no `GetAllNodes()`. This is the one additive change needed to allow
  `BoardView` to pull pre-existing nodes (placed by `GearBootstrap`) when it binds.
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
  **Rationale:** `GearBootstrap` seeds `IGridManager` with nodes before the screen opens.
  When `BoardView.Bind()` is called later, the board needs to pull what is already on the
  grid and spawn visuals after it has subscribed to runtime events. No other way to read
  that state exists without touching `IGridManager`.
  **Author:** this plan

- **Decision:** Do not fire initial `OnGearPlaced` events from `BoardViewModel.Initialize()`.
  `BoardView.Bind()` will subscribe first, then bind the current node collection explicitly.
  **Rationale:** `GearEngineViewModel.Initialize()` runs before `GearEngineView.OnBind()`.
  If `BoardViewModel.Initialize()` emitted initial placement events immediately, they would
  be lost before `BoardView` subscribed. Binding the collection from `BoardView.Bind()`
  gives one deterministic initialization flow and avoids event replay logic.
  **Author:** this plan

- **Decision:** `BoardView` owns a private `Dictionary<Vector2Int, GearView>` for the
  currently spawned visuals.
  **Rationale:** `OnGearRemoved(Vector2Int)` is a simple, stable event surface, but once a
  node is removed from `IGridManager` there is no longer a reliable way to retrieve the
  removed `IGridNode` for `GearViewFactory.GetView(node)`. A coord-keyed lookup inside
  `BoardView` keeps removal deterministic without widening the ViewModel event payloads.
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

---

## Outcomes & Retrospective

_(Fill in at completion: what shipped, what was deferred, lessons learned.)_

---

## Context and Orientation

### Files changed by this plan

| File | Change |
|---|---|
| `Manager/IGridManager.cs` | Add `IEnumerable<IGridNode> GetAllNodes()` |
| `Manager/GridManager.cs` | Implement `GetAllNodes()` — return internal dict values |
| `Presentation/UI/BoardViewModel.cs` | Remove `GearViewFactory` + `GearInventoryViewModel` params; add `OnGearPlaced` / `OnGearRemoved` events; stop touching views directly; expose `BoardConfig`, `GetNode`, and `GetCurrentNodes()` |
| `Presentation/UI/BoardView.cs` | Change base to `MonoBehaviour`; add `Bind(vm, interactable)` / `Unbind()`; own local `GearViewFactory`; maintain private `Dictionary<Vector2Int, GearView>`; react to VM events; expose `OnGearDroppedOverUI`; hold private `[SerializeField] GearBoardDragHandler dragHandler`; do all layout math |
| `Bootstrap/GearViewFactory.cs` | Remove `BoardConfigSO` constructor dependency; accept precomputed spawn position; only create/register `GearView` host objects |
| `Bootstrap/GearBootstrap.cs` | Remove `GearViewFactory` from `Construct`; `SpawnGear` creates node only, no view |
| `Bootstrap/GearMechanicsInstaller.cs` | Remove `GearViewFactory` singleton registration |
| `Presentation/GearEngineViewModel.cs` | Remove `[Inject] GearViewFactory`; keep only dependencies still needed; update `Board.Initialize()` call |
| `Presentation/GearEngineView.cs` | Call `boardView.Bind(viewModel.Board, interactable: true)`; subscribe `boardView.OnGearDroppedOverUI`; add `GearDroppedFromUIEvent` bridge — no `boardDragHandler` field |
| `Tests/Editor/BoardViewModelTests.cs` | Update to new `Initialize()` signature; add event-based tests |
| `Editor/SetupTestSceneTool.cs` | Add `GearBoardDragHandler` component to `GridBoardCollider` and wire it into `BoardView`'s private `dragHandler` field; update `GearEngineView` wiring (no `boardDragHandler` field) |
| `Docs/README.md` | Update composable scene setup section |

### New files created by this plan

| File | Purpose |
|---|---|
| `Presentation/UI/GearBoardDragHandler.cs` | All pointer/touch drag logic extracted from `BoardView.Update()` |

### No new interfaces, no new structs, no new display-only controllers.

---

## Plan of Work

1. **M1 — Grid enumeration**
   Add `GetAllNodes()` to the `IGridManager` interface and implement it in `GridManager`.

2. **M2 — Trim `BoardViewModel`**
   Remove `GearViewFactory` and `GearInventoryViewModel` from `Initialize()`. Replace all
   direct `viewFactory.*` calls with two C# event fires (`OnGearPlaced`, `OnGearRemoved`).
   Remove `GearDroppedFromUIEvent` subscription. Remove `boardVisualRoot` — the view owns
   its own transform. Expose `BoardConfig`, `GetNode(coord)`, and `GetCurrentNodes()` so
   the view can pull current state and apply layout math itself.

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

5. **M5 — Factory + bootstrap cleanup**
   Remove `GearViewFactory` singleton from `GearMechanicsInstaller`. Remove
   `GearViewFactory viewFactory` from `GearBootstrap.Construct()`. Make `SpawnGear`
   create the node and add it to the grid; nothing else.

6. **M6 — Screen wiring**
   In `GearEngineView.OnBind()`: call `boardView.Bind(viewModel.Board, interactable: true)` and wire
   both inventory bridges (`GearDroppedFromUIEvent` and `OnGearDroppedOverUI`). Update
   `GearEngineViewModel` to remove the unused `[Inject]` fields.

7. **M7 — Tests, Docs, editor tools**
   Update `BoardViewModelTests` to use the new signature. Add event-based tests. Update
   `SetupTestSceneTool`. Update `README.md`.

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
public IEnumerable<IGridNode> GetAllNodes() => _nodes.Values;
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
// View subscribes to these. No Unity types in the payload.
public event Action<Vector2Int, GearConfigData> OnGearPlaced;
public event Action<Vector2Int>                 OnGearRemoved;
```

**Expose helpers the view can use after it binds:**

```csharp
public BoardConfigSO BoardConfig => boardConfig;
public IGridNode GetNode(Vector2Int coord) => gridManager.GetNode(coord);
public IEnumerable<IGridNode> GetCurrentNodes() => gridManager.GetAllNodes();
```

Do **not** fire initial `OnGearPlaced` events inside `Initialize()`. `BoardView.Bind()`
subscribes first, then explicitly binds the current node collection via `GetCurrentNodes()`.

**Replace all `viewFactory.*` calls with event fires:**

Every place `BoardViewModel` currently calls `viewFactory.CreateView(...)` becomes
`OnGearPlaced?.Invoke(coord, configData)`.

Every place it calls `viewFactory.UnregisterView(node)` + `DestroyViewGameObject(go)`
becomes `OnGearRemoved?.Invoke(coord)`.

The view is responsible for the actual spawn and destroy — the ViewModel only declares
that a placement or removal occurred.

**Concrete replacements:**

| Old call in BoardViewModel | New code |
|---|---|
| `viewFactory.CreateView(newNode, data, parent)` | `OnGearPlaced?.Invoke(toPos, data)` |
| `DestroyGearViewForNode(node)` + view destroy | `OnGearRemoved?.Invoke(node.Position)` |
| `viewFactory.GetView(node)?.RecalculateRotationOffset()` | Remove — view recalculates in `GearView.Update()` or on receipt of `OnGearPlaced` |

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
private readonly Dictionary<Vector2Int, GearView> viewsByCoord = new();

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
    => viewsByCoord.Values;

internal bool IsRunning() => viewModel?.EngineService?.IsRunning ?? false;
internal BoardConfigSO GetBoardConfig() => viewModel?.BoardConfig;
```

These `internal` methods are the complete API surface between the drag handler and the
rest of the board. No external class can see or call them.

**Event handlers that drive spawning:**

```csharp
private void HandleGearPlaced(Vector2Int coord, GearConfigData config)
{
    IGridNode node = viewModel.GetNode(coord);
    if (node == null) { Debug.LogError($"[BoardView] No node at {coord} after OnGearPlaced."); return; }
    SpawnView(node);
}

private void HandleGearRemoved(Vector2Int coord)
{
    if (!viewsByCoord.TryGetValue(coord, out GearView view))
        return;

    viewsByCoord.Remove(coord);
    DestroyGO(view.gameObject);
}

private void SpawnView(IGridNode node)
{
    if (node == null) return;
    if (viewsByCoord.ContainsKey(node.Position)) return;

    Vector3 localPosition = viewModel.BoardConfig.GetWorldPosition(node.Position);
    GearView view = localFactory.CreateView(node, node.ConfigData, transform, localPosition);
    view.Initialize(node, node.ConfigData, viewModel.BoardConfig, localFactory);
    viewsByCoord[node.Position] = view;
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

### Milestone 5 — Factory singleton removal + `GearBootstrap` cleanup

**File: `Bootstrap/GearMechanicsInstaller.cs`**

Delete the line:

```csharp
builder.Register<GearViewFactory>(Lifetime.Singleton);
```

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

**File: `Bootstrap/GearBootstrap.cs`**

Remove `GearViewFactory viewFactory` from `[Inject] Construct(...)`:

```csharp
// Before
[Inject]
public void Construct(IGridManager grid, GearNodeFactory nodeFactory,
                      GearViewFactory viewFactory, BoardConfigSO boardConfig)

// After
[Inject]
public void Construct(IGridManager grid, GearNodeFactory nodeFactory, BoardConfigSO boardConfig)
```

Remove `viewFactory.CreateView(...)` from `SpawnGear()`:

```csharp
private void SpawnGear(Vector2Int pos, Transform parent)
{
    GearConfigData runtimeData = ResolveConfig();
    IGridNode node = nodeFactory.CreateNode(pos, runtimeData);
    grid.AddNode(node);
    // No view creation — BoardView.Bind() pulls current nodes via
    // BoardViewModel.GetCurrentNodes() and spawns visuals after subscribing.
}
```

---

### Milestone 6 — Screen wiring

**File: `Presentation/GearEngineViewModel.cs`**

Remove `[Inject] private GearViewFactory viewFactory` (the board no longer needs it from
the parent VM). Keep only the dependencies still used by the parent view model.

Update `Board.Initialize()` call to match the new 4-param signature:

```csharp
Board.Initialize(engineService, gridManager, nodeFactory, boardConfig);
```

**File: `Presentation/GearEngineView.cs`**

`GearEngineView` only holds a reference to `BoardView`. `GearBoardDragHandler` is
invisible here — it is wired privately inside the `BoardView` prefab.

```csharp
[SerializeField] private SimulationControlView simControlView;
[SerializeField] private GearInventoryView inventoryView;
[SerializeField] private BoardView boardView;   // ← the only board reference
// No boardDragHandler field. The drag handler is BoardView's internal concern.
```

Wire everything in `OnBind()`:

```csharp
protected override void OnBind()
{
    inventoryView.SetObjectResolver(viewModel.ObjectResolver);
    simControlView.Bind(viewModel.SimControl);
    inventoryView.Bind(viewModel.Inventory);
    boardView.Bind(viewModel.Board, interactable: true);

    // Bridge 1: board gear dragged over UI → return to inventory
    // OnGearDroppedOverUI lives on BoardView — no drag handler reference needed.
    boardView.OnGearDroppedOverUI += HandleGearDroppedOverUI;

    // Bridge 2: inventory slot dragged onto board → consume from inventory, place on board
    if (eventController != null)
        eventController.AddListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
}

private void HandleGearDroppedOverUI(GearConfigData config, Vector3 _)
{
    viewModel.Inventory.AddGearToInventory(config);
}

private void HandleGearDroppedFromUI(GearDroppedFromUIEvent ctx)
{
    try
    {
        if (viewModel.Board.EngineService.IsRunning) return;
        viewModel.Board.HandleInventoryDrop(ctx.WorldPosition, ctx.GearData, viewModel.Inventory);
    }
    catch (Exception ex)
    {
        Debug.LogError($"[GearEngineView] HandleGearDroppedFromUI failed: {ex.Message}\n{ex.StackTrace}");
    }
}

private void OnDestroy()
{
    boardView.OnGearDroppedOverUI -= HandleGearDroppedOverUI;
    if (eventController != null)
        eventController.RemoveListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
    boardView?.Unbind();
}
```

**Note on `HandleInventoryDrop`:**

The existing `HandleGearDroppedFromUI` logic (merge or place gear from UI drop) stays
in `BoardViewModel` — it is data-layer decision logic. Rename it to `HandleInventoryDrop`
with the signature:

```csharp
public void HandleInventoryDrop(
    Vector3 worldPosition,
    GearConfigData gearData,
    GearInventoryViewModel inventory)
```

This keeps the decision logic in the ViewModel while keeping the event subscription in
the view. The `inventory` parameter is passed in at call time rather than stored on the
ViewModel permanently — so the board itself still carries no permanent inventory reference.

`GearEngineView` injects `EventController` via `[Inject]` (it is registered in the scope).

---

### Milestone 7 — Tests, Docs, editor tools

#### Tests

**Update `Tests/Editor/BoardViewModelTests.cs`:**

Change `Board.Initialize()` calls to the new 4-param signature. Existing behaviour tests
(`OnGearPickedUp_ExtractsNodeFromGrid`, swap, merge, snap-back, out-of-bounds) still test
the same logic — the only difference is that assertions check event invocations instead
of checking `viewFactory.GetView()` calls.

Add the following new tests:

- `GetCurrentNodes_ReturnsExistingGridNodes` — seed a mock `IGridManager.GetAllNodes()`
  with two nodes; call `Initialize()`; assert `GetCurrentNodes()` yields both nodes.
- `PlaceNode_FiresOnGearPlaced` — call a path that results in a new node being added;
  assert the event fires.
- `RemoveNode_FiresOnGearRemoved` — assert the event fires with the correct coord.
- `BoardView_Bind_SpawnsViewsForExistingNodes` — if practical as an EditMode test,
  create a `BoardView`, bind a ViewModel whose grid already contains nodes, and assert
  the expected number of `GearView` children are created.

#### Docs

**Update `Docs/README.md`** — composable scene setup section:

- Note that `BoardView` is now a standalone `MonoBehaviour`; `GearBoardDragHandler` is
  optional. Show the minimal setup for each use case.
- Remove mention of `GearViewFactory` as a registered singleton.
- Document that inventory bridging lives in the screen view, not the board.

#### Editor tools

**Update `Editor/SetupTestSceneTool.cs`:**

- After `floorGrid.AddComponent<BoardView>()`, also add
  `floorGrid.AddComponent<GearBoardDragHandler>()`.
- Wire the private `dragHandler` field on `BoardView` using `SerializedObject` — same
  pattern used for all other `[SerializeField]` wiring in the tool.
- Update `GearEngineView` wiring: only the `boardView` field — no `boardDragHandler` field
  exists on `GearEngineView`.

---

## Validation and Acceptance

1. Run `.agents/scripts/validate-changes.cmd` — gate must be clean.
2. All tests in `Game.GearEngine.Tests` pass.
3. Open `Gear_Clean.unity` in Play Mode:
   - Gears appear at startup (seeded by `GearBootstrap`, picked up via `GetAllNodes()`).
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
```

### After (this plan)

```
BoardViewModel.Initialize(
    IGearEngineService,   // ✓
    IGridManager,         // ✓
    GearNodeFactory,      // ✓
    BoardConfigSO)        // ✓
  + BoardConfig property   // ← view reads config, but does layout math itself
  + OnGearPlaced event     // ← view reacts
  + OnGearRemoved event    // ← view reacts
  + GetNode(coord)         // ← thin helper, delegates to gridManager
  + GetCurrentNodes()      // ← one-time bind of current collection

BoardView : MonoBehaviour                   ← standalone; any scene, any prefab
  Bind(BoardViewModel, interactable) / Unbind()
  public event OnGearDroppedOverUI           ← only public surface screens subscribe to
  [SerializeField] private GearBoardDragHandler dragHandler  ← wired in prefab, invisible externally
  private localFactory = new GearViewFactory()               ← local, not shared
  private viewsByCoord dictionary                            ← stable runtime lookup for visuals
  Reacts to: OnGearPlaced → spawn GearView
             OnGearRemoved → destroy GearView
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
  eventController.GearDroppedFromUIEvent → viewModel.Board.HandleInventoryDrop(...)
  // No boardDragHandler reference on GearEngineView at all

GearViewFactory : plain local helper
  No BoardConfigSO dependency
  Accepts precomputed localPosition from BoardView
  Creates/registers GearView host objects only

GearBootstrap → grid.AddNode() only         ← no views

GearMechanicsInstaller → no GearViewFactory registration

Entry points:
  1. Scene startup: GearBootstrap seeds IGridManager (data nodes only)
  2. Screen open:   GearEngineView.OnBind() → boardView.Bind(viewModel.Board, interactable: true)
                    → BoardView subscribes to runtime events
                    → BoardView iterates viewModel.GetCurrentNodes()
                    → BoardView computes positions via viewModel.BoardConfig
                    → BoardView spawns initial GearViews
  3. Standalone:    anyMonoBehaviour → boardView.Bind(someViewModel, interactable: false) — no parent required
```

## Dependency Graph

```
GearEngineView
  ├── SimulationControlView
  ├── GearInventoryView
  └── BoardView
        ├── BoardViewModel
        │     ├── IGearEngineService
        │     ├── IGridManager
        │     ├── GearNodeFactory
        │     └── BoardConfigSO
        ├── GearViewFactory (local instance, created in Bind)
        ├── Dictionary<Vector2Int, GearView> (local runtime registry)
        └── GearBoardDragHandler (private serialized sibling reference)

GearBoardDragHandler
  └── BoardView only

GearViewFactory
  └── no DI dependencies

GearBootstrap
  ├── IGridManager
  ├── GearNodeFactory
  └── BoardConfigSO
```

## Initialization / Usage Flow

### Initialization flow

1. `GearMechanicsScope` builds the container and registers `IGridManager`,
   `IGearEngineService`, `GearNodeFactory`, `BoardConfigSO`, and related services.
2. `GearBootstrap.Initialize()` creates the starting logical nodes and adds them to
   `IGridManager`. It does not create any views.
3. `GearEngineNavigationEntry` opens `GearEngineViewModel`.
4. `GearEngineViewModel.Initialize()` creates and initializes `BoardViewModel` with
   `engineService`, `gridManager`, `nodeFactory`, and `boardConfig`.
5. `GearEngineView.OnBind()` calls `boardView.Bind(viewModel.Board, interactable: true)`.
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
   - inventory drop handling via `HandleInventoryDrop(...)`
8. `BoardViewModel` emits `OnGearPlaced` / `OnGearRemoved`.
9. `BoardView` reacts by updating `viewsByCoord`, creating or destroying `GearView`
   objects through its local `GearViewFactory`.
10. `GearEngineView` listens only to `boardView.OnGearDroppedOverUI` and
    `GearDroppedFromUIEvent` to bridge inventory interaction at the screen level.
