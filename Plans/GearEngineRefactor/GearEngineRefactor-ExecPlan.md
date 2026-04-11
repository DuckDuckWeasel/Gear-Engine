# GearEngine Refactor — Single Entrypoint, MVVM Composition, Simplified DI

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`,
`Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

Repository planning rules live at `PLANS.md`. This document must be maintained in
accordance with `PLANS.md`.

---

## Purpose / Big Picture

The GearEngine module currently has four loosely-connected entry points: `GearBootstrap`
(grid init), `BoardView` (board controls + drag logic), `GearMechanicsInstaller`
(MonoBehaviour installer), and separate `SimulationControlView`/`GearInventoryView`
components with no parent. External systems that want to interact with GearEngine must
inject `IGridManager` directly — an internal type — and must know about each sub-view
independently.

After this refactor:

- **One interface** (`IGearEngineService`) is the only thing external systems ever inject.
- **One parent view** (`GearEngineView`) opened via `INavigator` is the only UI entry point.
- **Sub-views** are `ViewComponent<T>` composites, not standalone navigation screens.
- **`BoardView`** owns all visual drag mechanics (pointer tracking, sprite following). It
  calls `BoardViewModel` only at decision points: pickup and drop.
- **`BoardViewModel`** owns grid/inventory decisions: where does the gear go, does it
  merge, does it snap back. No frame-by-frame input delegation.
- **`GearMechanicsInstaller`** becomes a plain C# class; `GearMechanicsScope` holds the
  scene refs and calls it.
- **`GearBootstrap`** is simplified: one `GearConfig initialGear` field (not an array),
  grid-only responsibility (inventory population moves to `GearEngineViewModel`).
- **Initial inventory** is driven by a `GearInventoryLoadoutSO` SO asset registered in the
  scope and injected into `GearEngineViewModel`.

**How to see it working:** open `Gear_Clean.unity`, enter Play Mode. The gear board
initialises from `initialGear`, the inventory populates from `GearInventoryLoadoutSO`,
and Play/Stop is reachable via `IGearEngineService` from any other system. The
`GearEngineView` opens via `INavigator`.

---

## Progress

- [ ] M1 — `IGearSceneElement`, `IGearEngineService`, `GearEngineService`, `GearInventoryLoadoutSO`, `GearBootstrap` cleanup
- [ ] M2 — `GearEngineViewModel` (parent ViewModel, child VMs updated)
- [ ] M3 — `GearEngineView` + sub-views converted to `ViewComponent<T>`
- [ ] M4 — `BoardViewModel` extraction; `BoardView` owns drag visuals, calls VM at decision points
- [ ] M5 — Installer + Scope simplification
- [ ] M6 — Navigation integration (`GearEngineViewConfig` asset)
- [ ] M7 — Tests, Docs, editor tool updates
- [ ] Quality gate passes (`validate-changes.cmd`)

---

## Surprises & Discoveries

- (fill in during implementation)

---

## Decision Log

- **Decision:** Introduce `IGearEngineService` as the external-facing facade, not expose
  `IGridManager` directly.
  **Rationale:** `IGridManager` is an internal simulation detail. Callers like `RaceViewModel`
  only need `Play()`/`Stop()`/`IsRunning`. Hiding the grid behind a service facade means
  internal changes never ripple to external modules.
  **Author:** initial plan

- **Decision:** Keep `GearBootstrap` as a MonoBehaviour but add `IGearSceneElement`.
  **Rationale:** `GearBootstrap` parents grid visuals in scene space — it needs a
  `Transform`. The interface makes it injectable and testable without removing Unity
  lifecycle.
  **Author:** initial plan

- **Decision:** Replace `GearConfig[] gearConfigs` with `GearConfig initialGear` (single field).
  **Rationale:** `PopulateGrid()` only ever spawns one gear (the core at centre). The
  non-core array slot was dead code. Flatten now; re-introduce a collection if the design
  changes.
  **Author:** initial plan

- **Decision:** Extract `GearInventoryLoadoutSO` for starting inventory.
  **Rationale:** `GearConfig[] startingInventoryGears` baked into `GearBootstrap` makes
  loadout swapping impossible without touching the prefab. A SO lets you have multiple
  loadouts (`DefaultLoadout`, `RaceLoadout`, etc.) selectable in the Inspector.
  **Author:** initial plan

- **Decision:** Inventory population moves from `GearBootstrap` to `GearEngineViewModel.Initialize()`.
  **Rationale:** `GearBootstrap` is a scene-element that owns grid visuals; it should not
  know about `GearInventoryViewModel`. `GearEngineViewModel` owns all child ViewModels and
  is the correct place to seed initial state. `GearInventoryLoadoutSO` is registered in
  the scope and injected into `GearEngineViewModel` via `[Inject]`.
  **Author:** initial plan revision

- **Decision:** Sub-views become `ViewComponent<T>`, not `View<T>`.
  **Rationale:** `SimulationControlView`, `GearInventoryView`, and `BoardView` are
  composable widgets within one screen — not navigation screens in their own right. The
  parent `GearEngineView` is the screen.
  **Author:** initial plan

- **Decision:** `BoardView` owns all drag visuals; `BoardViewModel` is called only at decision points.
  **Rationale:** Dragging is visual feedback — moving a sprite is a view concern. `BoardView`
  handles pointer tracking, proximity detection, and sprite repositioning entirely. It
  calls `BoardViewModel` only when a meaningful grid event occurs: `OnGearPickedUp` and
  `OnGearDropped`. `BoardViewModel` then decides what that means for the grid and
  inventory (place, swap, merge, snap back, return to inventory). This removes the
  `PointerState` struct and the frame-by-frame `Tick` delegation, keeping the boundary
  event-driven and proportional.
  **Author:** initial plan revision

- **Decision:** `GearMechanicsInstaller` becomes a plain C# class.
  **Rationale:** Removes the `GetComponent<GearMechanicsInstaller>()` coupling in
  `GearMechanicsScope`. Serialised scene refs belong in the Scope, not in a second
  component on the same GameObject.
  **Author:** initial plan

- **Decision:** No ViewModels, Views, or ViewConfigs are registered in the scope.
  **Rationale:** Navigation creates `GearEngineView` on demand from the `ViewConfig` asset
  and injects into it automatically. `GearEngineViewModel` is created with `new` by the
  framework — its constructor takes no DI params. Child ViewModels are created with `new`
  inside the parent and receive services via `Initialize()` called from
  `GearEngineViewModel.Initialize()`. Registering them in the container would couple
  Navigation lifetime to the DI container unnecessarily.
  **Author:** initial plan revision

- **Decision:** Use `INavigator` in `GearEngineViewModel` instead of any custom show/hide helpers.
  **Rationale:** Consistent with the rest of the project. No new utility classes needed.
  **Author:** initial plan

---

## Outcomes & Retrospective

(Summarise at completion: what shipped, what was deferred, lessons learned.)

---

## Context and Orientation

### Existing files touched by this plan

| File | Change |
|---|---|
| `Bootstrap/GearBootstrap.cs` | Implements `IGearSceneElement`; `GearConfig[]` → `GearConfig initialGear`; remove `startingInventoryGears` and `inventoryViewModel` inject entirely (inventory moves to `GearEngineViewModel`) |
| `Bootstrap/GearMechanicsInstaller.cs` | Remove `MonoBehaviour`; become plain C# class; remove all view/viewmodel fields and registrations |
| `Bootstrap/GearMechanicsScope.cs` | Hold only scene-element and SO refs; construct `GearMechanicsInstaller` directly |
| `Presentation/UI/SimulationControlView.cs` | `View<T>` → `ViewComponent<T>`; remove `[Inject] Construct` |
| `Presentation/UI/SimulationControlViewModel.cs` | Remove `[Inject]`; add `Initialize(IGearEngineService)` called by parent VM |
| `Presentation/UI/GearInventoryView.cs` | `View<T>` → `ViewComponent<T>`; remove `[Inject] Construct` |
| `Presentation/UI/GearInventoryViewModel.cs` | Remove `[Inject]`; add `Initialize(IGearEngineService)` called by parent VM |
| `Presentation/UI/BoardView.cs` | `ViewComponent<BoardViewModel>`; owns all drag visuals; calls `OnGearPickedUp`/`OnGearDropped` on VM at decision points only |
| `Bootstrap/GearViewFactory.cs` | Add view registry so `BoardViewModel` can look up views by node |
| `Editor/SetupBasicConfigsTool.cs` | Generate `GearInventoryLoadout.asset`; wire `initialGear` (not array) on `GearBootstrap` |
| `Editor/SetupTestSceneTool.cs` | Remove `GearMechanicsInstaller` component wiring; serialise only scene-element refs on Scope |
| `Tests/Editor/GearEngineFlowTests.cs` | Add `GearEngineService` and `BoardViewModel` test cases |
| `Docs/README.md` | Reflect new single-entrypoint design |

### New files created by this plan

| File | Purpose |
|---|---|
| `IGearSceneElement.cs` | Interface on scene-resident bootstrap (`Initialize`, `Enable`, `Disable`) |
| `IGearEngineService.cs` | External-facing facade (`IsRunning`, `Play`, `Stop`) |
| `GearEngineService.cs` | Plain C# implementation; wraps `IGridManager` + `IGearSceneElement` |
| `Config/GearInventoryLoadoutSO.cs` | SO holding `GearConfig[]` starting gears; `[CreateAssetMenu]` |
| `Presentation/GearEngineViewModel.cs` | Parent ViewModel; creates child VMs with `new`; distributes services in `Initialize()` |
| `Presentation/GearEngineView.cs` | `View<GearEngineViewModel>`; opened by Navigation; calls `Bind(vm)` on sub-views |
| `Presentation/UI/BoardViewModel.cs` | Grid/inventory decision logic; receives services via `Initialize()`; called by `BoardView` at pickup and drop points only |

### Key types referenced (not in this repo source)

| Type | Package | Role |
|---|---|---|
| `View<T>` | `Scaffold.MVVM.View` | Full navigation-owned screen view |
| `ViewComponent<T>` | `Scaffold.MVVM.View` | Composable sub-view within a parent View |
| `ViewModel` | `Scaffold.MVVM.ViewModel` | Base class for all ViewModels |
| `INavigator` | `Scaffold.Navigation` | Opens/closes views via `ViewConfig` assets |
| `ITickable` | `VContainer.Unity` | Tick registration in VContainer |

---

## Plan of Work

1. **M1 — Scene element interface + service facade + config cleanup**
   Introduce `IGearSceneElement` on `GearBootstrap`. Create `IGearEngineService` and
   `GearEngineService`. Extract `GearInventoryLoadoutSO`. Simplify `GearBootstrap` to
   `initialGear` only (no inventory logic).

2. **M2 — Parent ViewModel**
   Create `GearEngineViewModel`. Convert `SimulationControlViewModel` and
   `GearInventoryViewModel` to plain `Initialize()` pattern (no `[Inject]`). Inventory
   population moves here from `GearBootstrap`.

3. **M3 — Parent View + sub-view conversion**
   Create `GearEngineView`. Convert `SimulationControlView`, `GearInventoryView`,
   `BoardView` to `ViewComponent<T>`. Parent view calls `Bind(vm)` on each child.

4. **M4 — BoardViewModel extraction**
   Create `BoardViewModel` with grid/inventory decision logic. `BoardView` owns all drag
   visuals and calls `OnGearPickedUp`/`OnGearDropped` on the ViewModel at decision points.
   Add view registry to `GearViewFactory`.

5. **M5 — Installer + Scope simplification**
   Remove MonoBehaviour from `GearMechanicsInstaller`. Scope holds only scene elements
   and SO assets. No ViewModels, Views, or ViewConfigs registered anywhere.

6. **M6 — Navigation integration**
   Create `GearEngineViewConfig.asset`. Confirm `INavigator` is available in the scope.
   Verify `GearEngineView` opens and closes via `navigator.OpenAsync/CloseAsync`.

7. **M7 — Tests, Docs, editor tool updates**
   Add `GearEngineServiceTests` and `BoardViewModelTests`. Update `README.md`. Update
   both editor setup tools.

---

## Concrete Steps

### Milestone 1 — Scene element interface + service facade + config cleanup

**New file: `Assets/Scripts/Game/GearEngine/IGearSceneElement.cs`**
```csharp
namespace Game.GearEngine
{
    public interface IGearSceneElement
    {
        void Initialize();
        void Enable();
        void Disable();
    }
}
```

**New file: `Assets/Scripts/Game/GearEngine/IGearEngineService.cs`**
```csharp
namespace Game.GearEngine
{
    public interface IGearEngineService
    {
        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
```

**New file: `Assets/Scripts/Game/GearEngine/GearEngineService.cs`**
```csharp
using System;

namespace Game.GearEngine
{
    public sealed class GearEngineService : IGearEngineService
    {
        private readonly IGridManager gridManager;
        private readonly IGearSceneElement sceneElement;

        public GearEngineService(IGridManager gridManager, IGearSceneElement sceneElement)
        {
            if (gridManager  == null) throw new ArgumentNullException(nameof(gridManager));
            if (sceneElement == null) throw new ArgumentNullException(nameof(sceneElement));
            this.gridManager  = gridManager;
            this.sceneElement = sceneElement;
        }

        public bool IsRunning => gridManager.IsRunning;
        public void Play()    => gridManager.Play();
        public void Stop()    => gridManager.Stop();
    }
}
```

**New file: `Assets/Scripts/Game/GearEngine/Config/GearInventoryLoadoutSO.cs`**
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(menuName = "GearEngine/Inventory Loadout", fileName = "GearInventoryLoadout")]
    public sealed class GearInventoryLoadoutSO : ScriptableObject
    {
        [SerializeField] private GearConfig[] startingGears;

        public IReadOnlyList<GearConfig> StartingGears => startingGears;
    }
}
```

**Modify `GearBootstrap.cs`:**
- Implement `IGearSceneElement`.
- Replace `[SerializeField] private GearConfig[] gearConfigs` with
  `[SerializeField] private GearConfig initialGear`.
- Remove `[SerializeField] private GearConfig[] startingInventoryGears` entirely —
  inventory population moves to `GearEngineViewModel`.
- Simplify `[Inject] Construct(...)` — remove `GearInventoryViewModel inventoryViewModel`
  parameter; `GearBootstrap` only needs grid services:
  ```csharp
  [Inject]
  public void Construct(IGridManager grid, GearNodeFactory nodeFactory,
                        GearViewFactory viewFactory, BoardConfigSO boardConfig)
  ```
- Remove `PopulateStartingInventory()` entirely.
- Move `Start()` body to `Initialize()`. `Start()` calls `Initialize()`.
- Add `Enable() => gameObject.SetActive(true)` and `Disable() => gameObject.SetActive(false)`.
- Remove `ResolveConfig(bool isCore)`. Replace with:
  ```csharp
  private GearConfigData ResolveConfig()
  {
      if (initialGear != null) return initialGear.CreateRuntimeData();
      return new GearConfigData
      {
          Id = "core_default",
          Category = GearCategory.Core,
          BaseRotationSpeed = 90f,
          TriggerPattern = TriggerPattern.EightWay,
          MaxCharge = 100f,
          ChargeOverTimeAmount = 10f,
          ChargeOnTriggerAmount = 25f
      };
  }
  ```
- Update `SpawnGear()` — remove `bool isCore` parameter; call `ResolveConfig()`.
- Update `PopulateGrid()` — remove `isCore` branching; `SpawnGear` called only at centre.

---

### Milestone 2 — Parent ViewModel

**New file: `Assets/Scripts/Game/GearEngine/Presentation/GearEngineViewModel.cs`**

`GearEngineViewModel` is created on demand by Navigation — not registered in the
container. Its constructor takes no DI parameters. All registered services are received
via `[Inject]` fields. Child ViewModels are created with `new` and receive services
through explicit `Initialize()` calls in `GearEngineViewModel.Initialize()`.

```csharp
using VContainer;
using Scaffold.MVVM;
using Scaffold.Navigation;

namespace Game.GearEngine.Presentation
{
    public sealed class GearEngineViewModel : ViewModel
    {
        // Services injected by DI after construction
        [Inject] private IGearEngineService engineService;
        [Inject] private INavigator navigator;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private GearViewFactory viewFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private EventController eventController;
        [Inject] private GearInventoryLoadoutSO loadout;

        // Child ViewModels — owned by this ViewModel, not the DI container
        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();

        protected override void Initialize()
        {
            // Seed inventory from the SO loadout
            foreach (var config in loadout.StartingGears)
            {
                if (config != null)
                    Inventory.AddGearToInventory(config.CreateRuntimeData());
            }

            // Distribute services to child ViewModels
            SimControl.Initialize(engineService);
            Inventory.Initialize(engineService);
            Board.Initialize(engineService, gridManager, nodeFactory,
                             viewFactory, Inventory, boardConfig, eventController);
        }
    }
}
```

**Modify `SimulationControlViewModel.cs`:**
- Remove `[Inject] public void Construct(IGridManager gridManager)`.
- Add `public void Initialize(IGearEngineService engineService)` — store the reference,
  sync `IsRunning` from `engineService.IsRunning`.
- Replace all `gridManager.*` calls with `engineService.*`.

**Modify `GearInventoryViewModel.cs`:**
- Remove `[Inject] public void Construct(IGridManager gridManager)`.
- Add `public void Initialize(IGearEngineService engineService)` — store the reference.
- `CanDrag` becomes `=> !engineService.IsRunning`.

---

### Milestone 3 — Parent View + sub-view conversion

**New file: `Assets/Scripts/Game/GearEngine/Presentation/GearEngineView.cs`**

`GearEngineView` is a prefab opened by Navigation. Navigation injects the ViewModel into
it automatically. Sub-views are `[SerializeField]` references wired in the prefab
Inspector — they are not scene objects and are never registered in the scope.

```csharp
using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public class GearEngineView : View<GearEngineViewModel>
    {
        [SerializeField] private SimulationControlView simControlView;
        [SerializeField] private GearInventoryView inventoryView;
        [SerializeField] private BoardView boardView;

        protected override void OnBind()
        {
            simControlView.Bind(viewModel.SimControl);
            inventoryView.Bind(viewModel.Inventory);
            boardView.Bind(viewModel.Board);
        }
    }
}
```

**Modify `SimulationControlView.cs`:**
- Change base class: `View<SimulationControlViewModel>` → `ViewComponent<SimulationControlViewModel>`.
- Remove `[Inject] public void Construct(SimulationControlViewModel vm)` — the parent
  view calls `Bind(vm)` directly.
- `OnBind()` and `OnDestroy()` are unchanged.

**Modify `GearInventoryView.cs`:**
- Change base class: `View<GearInventoryViewModel>` → `ViewComponent<GearInventoryViewModel>`.
- Remove `[Inject] public void Construct(GearInventoryViewModel vm, IObjectResolver container)`.
- `OnBind()` and `OnDestroy()` are unchanged.
- The `IObjectResolver` used for injecting dynamically-created slot views should be
  received via a `[Inject]` field on `GearInventoryView` itself (not via the removed
  Construct method).

**Modify `BoardView.cs`:**
- Change base class to `ViewComponent<BoardViewModel>`.
- Remove the large `[Inject] Construct(...)` multi-dependency block.
- `OnBind()` caches `Camera.main`.
- `Update()` handles all visual drag mechanics — pointer tracking, proximity detection,
  sprite repositioning — entirely within `BoardView`. No per-frame delegation to the VM.
- Calls `viewModel.OnGearPickedUp(node, fromPos)` when a drag begins.
- Calls `viewModel.OnGearDropped(node, toPos, isOverUI)` when a drag ends.

---

### Milestone 4 — BoardViewModel extraction

**Responsibility split:**

| Concern | Owner |
|---|---|
| Pointer tracking (down/held/up) | `BoardView` |
| Proximity detection (which gear is closest) | `BoardView` |
| Sprite repositioning while dragging | `BoardView` |
| UI overlap detection | `BoardView` |
| Grid position snapping (screen → grid coord) | `BoardView` |
| Extract node from grid on pickup | `BoardViewModel.OnGearPickedUp` |
| Place / swap / merge / snap-back on drop | `BoardViewModel.OnGearDropped` |
| Return gear to inventory on drop-over-UI | `BoardViewModel.OnGearDropped` |
| Subscribe to `GearDroppedFromUIEvent` | `BoardViewModel` |

**New file: `Assets/Scripts/Game/GearEngine/Presentation/UI/BoardViewModel.cs`**

`BoardViewModel` is created with `new` by `GearEngineViewModel` and is not registered in
the container. It receives all dependencies via `Initialize()`.

```csharp
public sealed class BoardViewModel
{
    public void Initialize(
        IGearEngineService engineService,
        IGridManager gridManager,
        GearNodeFactory nodeFactory,
        GearViewFactory viewFactory,
        GearInventoryViewModel inventory,
        BoardConfigSO boardConfig,
        EventController eventController)
    {
        // store all references
        // subscribe to GearDroppedFromUIEvent via eventController
    }

    // Called by BoardView when the player lifts a gear off the board
    public void OnGearPickedUp(IGridNode node, Vector2Int fromPos)
    {
        // extract node from grid
    }

    // Called by BoardView when the player releases a drag
    public void OnGearDropped(IGridNode node, Vector2Int toPos, bool isOverUI)
    {
        // decide: place / swap / merge / snap-back / return to inventory
    }
}
```

`BoardViewModel` looks up views via `GearViewFactory`'s registry — no
`FindObjectsOfType<GearView>()`. Unsubscribes from events in `Dispose()`.

**Modify `GearViewFactory.cs`:**
- Add `private Dictionary<IGridNode, GearView> viewRegistry`.
- `CreateView(...)` registers the returned view: `viewRegistry[node] = view`.
- Add `GearView GetView(IGridNode node)`.
- Add `void UnregisterView(IGridNode node)` — call this wherever views are destroyed.
- `BoardViewModel` uses these instead of `FindObjectsOfType<GearView>()`.

**`BoardView.cs` after changes:**
```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using Scaffold.MVVM;

namespace Game.GearEngine.Presentation
{
    public class BoardView : ViewComponent<BoardViewModel>
    {
        private Camera mainCamera;
        private GearView draggedView;
        private Vector2Int originalGridPos;

        protected override void OnBind() => mainCamera = Camera.main;

        private void Update()
        {
            if (viewModel == null || mainCamera == null) return;
            if (engineService.IsRunning) return;

            Vector3 worldPos = GetWorldPointerPosition();

            if (IsPointerDown())   HandlePickup(worldPos);
            if (IsPointerHeld())   HandleDragHover(worldPos);
            if (IsPointerUp())     HandleDrop(worldPos);
        }

        private void HandlePickup(Vector3 worldPos)
        {
            // proximity detection — find closest interactable GearView
            // set draggedView, store originalGridPos
            // call viewModel.OnGearPickedUp(node, originalGridPos)
        }

        private void HandleDragHover(Vector3 worldPos)
        {
            if (draggedView != null)
                draggedView.transform.position = worldPos;  // pure visual
        }

        private void HandleDrop(Vector3 worldPos)
        {
            bool overUI   = EventSystem.current != null
                         && EventSystem.current.IsPointerOverGameObject();
            Vector2Int toPos = boardConfig.GetGridPosition(worldPos);
            viewModel.OnGearDropped(draggedView.TargetNode, toPos, overUI);
            draggedView = null;
        }

        // ... pointer helpers (IsPointerDown, GetWorldPointerPosition, etc.)
    }
}
```

Note: `BoardView` needs `BoardConfigSO` for grid-position snapping. Receive it via
`[Inject]` field — it is registered in the scope.

---

### Milestone 5 — Installer + Scope simplification

The installer registers **only services and scene-resident instances**. No ViewModels,
Views, or ViewConfigs are registered. Navigation manages the view lifecycle; child
ViewModels are owned by `GearEngineViewModel`.

**Modify `GearMechanicsInstaller.cs`** — remove `MonoBehaviour`:
```csharp
using System;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public sealed class GearMechanicsInstaller
    {
        private readonly BoardConfigSO boardConfig;
        private readonly GearBootstrap bootstrap;
        private readonly GearInventoryLoadoutSO loadout;

        public GearMechanicsInstaller(
            BoardConfigSO boardConfig,
            GearBootstrap bootstrap,
            GearInventoryLoadoutSO loadout)
        {
            if (boardConfig == null) throw new ArgumentNullException(nameof(boardConfig));
            if (bootstrap   == null) throw new ArgumentNullException(nameof(bootstrap));
            if (loadout     == null) throw new ArgumentNullException(nameof(loadout));
            this.boardConfig = boardConfig;
            this.bootstrap   = bootstrap;
            this.loadout     = loadout;
        }

        public void Install(IContainerBuilder builder)
        {
            // Scene instances and SO assets
            builder.RegisterInstance(boardConfig);
            builder.RegisterInstance(loadout);
            builder.RegisterInstance(bootstrap).As<IGearSceneElement>().AsSelf();

            // Services
            builder.Register<EventController>(Lifetime.Singleton)
                   .AsImplementedInterfaces().AsSelf();
            builder.Register<GridManager>(Lifetime.Singleton)
                   .AsImplementedInterfaces().AsSelf();
            builder.Register<GearEngineService>(Lifetime.Singleton)
                   .As<IGearEngineService>();

            // Node types (Transient — GearNodeFactory resolves these)
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            // Factories and domain services
            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<GearViewFactory>(Lifetime.Singleton);

            // No ViewModels — created on demand by Navigation / GearEngineViewModel
            // No Views      — Navigation manages their lifecycle via ViewConfig
            // No ViewConfigs — assets, not runtime registrations
        }
    }
}
```

**Modify `GearMechanicsScope.cs`:**
```csharp
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public class GearMechanicsScope : LifetimeScope
    {
        [SerializeField] private BoardConfigSO boardConfig;
        [SerializeField] private GearBootstrap bootstrap;
        [SerializeField] private GearInventoryLoadoutSO loadout;

        // No views, no ViewModels, no ViewConfigs

        protected override void Configure(IContainerBuilder builder)
        {
            var installer = new GearMechanicsInstaller(boardConfig, bootstrap, loadout);
            installer.Install(builder);
        }
    }
}
```

---

### Milestone 6 — Navigation integration

- Turn the `GearEngineView` GameObject into a prefab.
- Mark the prefab as an Addressable entry (or use the direct-prefab path from the
  `NavigationViewConfig` plan if that is complete first).
- Create `Assets/Game/GearEngine/Configs/GearEngineViewConfig.asset` pointing at the prefab.
- Confirm `INavigator` is registered in the parent scope that contains
  `GearMechanicsScope` as a child, or register it in `GearMechanicsInstaller` if the
  scope is standalone.
- Acceptance check: enter Play Mode → `GearEngineView` opens cleanly via
  `navigator.OpenAsync(gearEngineViewConfig)`; `navigator.CloseAsync()` tears it down
  without errors.

---

### Milestone 7 — Tests, Docs, editor tool updates

**New test class: `GearEngineServiceTests.cs`** (in the existing `Game.GearEngine.Tests` assembly)
- `Play_DelegatesToGridManager` — mock `IGridManager`; call `service.Play()`; assert
  `gridManager.Play()` was called.
- `Stop_DelegatesToGridManager` — same for `Stop()`.
- `IsRunning_ReflectsGridManagerState` — assert facade property mirrors mock value.

**New test class: `BoardViewModelTests.cs`** (same assembly)
- `OnGearPickedUp_ExtractsNodeFromGrid`
- `OnGearDropped_EmptySlot_AddsNodeAtNewPosition`
- `OnGearDropped_OccupiedSlot_SameId_WithNextLevel_MergesGear`
- `OnGearDropped_OutOfBounds_SnapsBackToOriginalPosition`
- `OnGearDropped_OverUI_ReturnsGearToInventory`

All tests use plain `new BoardViewModel()` + `Initialize(mocks...)` and call
`OnGearPickedUp`/`OnGearDropped` directly — no MonoBehaviour, no Camera, no scene required.

**Update `SetupBasicConfigsTool.cs`:**
- Create and save a `GearInventoryLoadout.asset` with the default starting gears.
- Wire the single `initialGear` field (not an array) on `GearBootstrap`.

**Update `SetupTestSceneTool.cs`:**
- Remove `GearMechanicsInstaller` MonoBehaviour component creation.
- Serialise `boardConfig`, `bootstrap`, and `loadout` directly on `GearMechanicsScope`
  (three fields only — no view or ViewModel refs).

**Update `Docs/README.md`:**
- Replace the "Composable scene setup" section to reflect the new scope/installer shape.
- Document `IGearEngineService` as the external integration point.
- Document `GearInventoryLoadoutSO` and where to place new loadout assets.

---

## Validation and Acceptance

1. Run `.agents/scripts/validate-changes.cmd` — gate must be clean.
2. All new tests pass in EditMode (`Game.GearEngine.Tests`).
3. Open `Gear_Clean.unity` in Play Mode:
   - Grid initialises from `initialGear`.
   - Inventory populates from `GearInventoryLoadoutSO`.
   - Play/Stop button works.
   - Gear drag/drop/merge/swap on board works.
   - No `FindObjectsOfType` calls at runtime (verify with profiler or log search).
4. `IGearEngineService` is resolvable from a parent scope (manual DI smoke test).
5. No Roslyn analyzer warnings or errors.

---

## Idempotence and Recovery

- Each milestone is independently committable. Reverting a later milestone leaves earlier
  ones intact.
- `GearBootstrap.Initialize()` is idempotent when called from `Start()` — no state is
  duplicated if invoked twice in tests.
- The old `GearMechanicsInstaller` MonoBehaviour will produce a missing-component warning
  in existing scenes until M5 is applied. Update both `GearEngine_TestScene.unity` and
  `Gear_Clean.unity` as part of M5 to avoid leaving broken serialised references.

---

## Artifacts and Notes

- `Plans/GearEngineRefactor/GearEngineRefactor-ExecPlan.md` — this file
- `Assets/Scripts/Game/GearEngine/IGearEngineService.cs`
- `Assets/Scripts/Game/GearEngine/IGearSceneElement.cs`
- `Assets/Scripts/Game/GearEngine/GearEngineService.cs`
- `Assets/Scripts/Game/GearEngine/Config/GearInventoryLoadoutSO.cs`
- `Assets/Scripts/Game/GearEngine/Presentation/GearEngineViewModel.cs`
- `Assets/Scripts/Game/GearEngine/Presentation/GearEngineView.cs`
- `Assets/Scripts/Game/GearEngine/Presentation/UI/BoardViewModel.cs`

---

## Interfaces and Dependencies

```
IGearEngineService
  └── GearEngineService (plain C#, registered as singleton)
        ├── IGridManager       (registered, unchanged)
        └── IGearSceneElement  (registered, implemented by GearBootstrap)

GearEngineViewModel  (created on demand by Navigation, not registered)
  ├── [Inject] IGearEngineService
  ├── [Inject] INavigator
  ├── [Inject] IGridManager
  ├── [Inject] GearNodeFactory
  ├── [Inject] GearViewFactory
  ├── [Inject] BoardConfigSO
  ├── [Inject] EventController
  ├── [Inject] GearInventoryLoadoutSO
  ├── new SimulationControlViewModel  → Initialize(engineService)
  ├── new GearInventoryViewModel      → Initialize(engineService)
  └── new BoardViewModel              → Initialize(engineService, gridManager, nodeFactory,
                                                   viewFactory, inventory, boardConfig,
                                                   eventController)

GearEngineView : View<GearEngineViewModel>  (prefab, opened by Navigation)
  ├── [SerializeField] SimulationControlView  → ViewComponent<SimulationControlViewModel>
  ├── [SerializeField] GearInventoryView      → ViewComponent<GearInventoryViewModel>
  └── [SerializeField] BoardView              → ViewComponent<BoardViewModel>
        owns: pointer tracking, proximity detection, sprite drag, grid-pos snapping
        calls VM: OnGearPickedUp(node, fromPos) | OnGearDropped(node, toPos, isOverUI)
        [Inject] BoardConfigSO (for GetGridPosition)

GearMechanicsScope (LifetimeScope)
  └── GearMechanicsInstaller (plain C#)
        ├── [SerializeField] BoardConfigSO          → RegisterInstance
        ├── [SerializeField] GearBootstrap          → RegisterInstance as IGearSceneElement
        └── [SerializeField] GearInventoryLoadoutSO → RegisterInstance
```
