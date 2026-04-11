# Gear Engine

The **Gear Engine** module is a highly-performant, event-driven Puzzle/Grid architecture built strictly on SOLID principles. It controls the simulation, placement, continuous ticking, and spatial logic of interlocking components on a 2D integer grid.

## Architecture & Core Components

Instead of treating gears as physics objects, the system relies on a central spatial hash dictionary overseen by a `GridManager`.

### 1. The Manager & Tick Mechanics
*   **`GridManager` (and `IGridManager`)**: The central heartbeat of the engine. It registers all active nodes in a dictionary (`Dictionary<Vector2Int, IGridNode>`). It exposes `Play()` and `Stop()` methods.
*   **Simulation Tick**: When the engine is running (`IsRunning`), the `GridManager` continuously updates every `IGridNode` over time (`DeltaTime`) in its main `Tick()` loop. 
    *   **Pre-calculation Phase:** The tick resolves all active auras and modifiers first.
    *   **Execution Phase:** All nodes calculate their progression (e.g., rotation physics, charge buildup) based on independent speeds.
*   **Wind-Down**: When told to Stop, the engine halts standard ticking and enters a smooth "Wind-Down" loop that interpolates all gears back to stable orthogonal rest positions (0, 90, 180, 270 degrees) precisely, without abrupt visual snapping.

### 2. Snap Collisions & The Nodes (`IGridNode`)
All puzzle items on the board implement `IGridNode`. They follow `NodeBase`, which handles standard logic like tracking the `IsActive` state, rotation progress, and maintaining a `List<RuntimeAbility>` for temporary modifiers.

*   **`CoreGearNode`**: The active continuous engine/motor. As it spins during the tick, it watches a subset of "Trigger Degrees" based on its pattern configuration.
    *   **Snap Collision**: When the gear mechanically sweeps past an interactive degree threshold (e.g., passing 90°, 180°, 270°), the collision system registers a "mechanical snap/click". It calculates the exact neighboring cell coordinate facing that direction and fires a strict `DirectionalTriggerEvent` payload at that target.
*   **`BaseGearNode` (Standard Gear)**: The passive receiver. It listens for `DirectionalTriggerEvent`s matching its spatial coordinate. It accumulates `CurrentCharge` continuously and upon receiving triggers. When `MaxCharge` is reached, it fires all its configured abilities and resets. 
    *   **Concurrency Shield:** To prevent race conditions from simultaneous engines (e.g., 2 CoreGears triggering the same node in a single frame), a strict 1-execution-per-tick limit (`hasExecutedThisTick`) is enforced. Excess charge correctly accumulates but execution execution is naturally deferred to the immediate next physical tick in the queue.
*   **`AuraGearNode`**: The hazard/support gear. Instead of charging, it constantly emits effects (like `LocalSpeedMultiplier`) to neighboring cells during the pre-calculation phase of the Grid `Tick()`.

### 3. The Abilities System (Strategy Pattern)
The engine separates action logic from the Node class. All abilities are configured via the Unity Inspector using `ScriptableObject` assets that inherit from `GearAbilitySO`. This allows heavy reuse and composition.

*   **`GearAbilitySO`**: Contains pure virtual lifecycle hooks: `OnActive`, `Tick`, `OnDeactive` (for continuous effects/buffs), and `Execute` (the explosive burst when a BaseGear hits max charge).
*   **`RuntimeAbility`**: Because ScriptableObjects are stateless, this wrapper sits inside the Node carrying the reference to the SO alongside a `DurationRemaining` float, naturally allowing an ability to act as a timed buff/debuff.
*   **`DestroySelfAbility` / `InactiveAbility`**: Concrete examples. Obstacles are created without custom classes: you just make a `BaseGearNode`, set `IsInteractable = false`, and assign `DestroySelfAbility`. When it takes enough ticks from a Core Gear, it destroys itself.

### 4. Configuration Pipeline
*   **`GearConfig` / `GearConfigData`**: The immutable data definitions of gears. Specifies speeds, shapes, interaction flags, max charges, and holds lists of the `GearAbilitySO`s. The `MergeService` uses these config objects to level up gears gracefully without hard type dependencies to `MonoBehaviour` views.

---

## Composable scene setup (VContainer)

Gear mechanics are wired for reuse inside larger scenes:

* **`IGearEngineService` / `GearEngineService`**: External integration surface for simulation control (`Play` / `Stop` / `IsRunning`). Inject this from other modules instead of `IGridManager`.
* **`GearMechanicsInstaller`**: Plain C# installer constructed by `GearMechanicsScope`. Registers **`BoardConfigSO`**, grid/simulation services, and node factories. Does **not** register `GearViewFactory` (the board owns a local factory), views, or view models.
* **`GearMechanicsScope`**: `LifetimeScope` with **`NavigationSettings`**, **`navigationViewHolder`** (transform that parents the context `GearEngineView`), **`BoardConfigSO`**, and optional **`GearTestSceneBootstrap`**. Installs **`NavigationInstaller`**, **`AddressablesInstaller`**, **`CrossLayerObjectResolver`**, and **`GearMechanicsInstaller`**. The screen is opened from **`GearTestSceneBootstrap`** (`INavigation.Open(new GearEngineViewModel(startData))`) or from any host that supplies **`GearEngineStartData`**.
* **`GearEngineStartData` / `BoardLayoutData`**: Serializable startup payload for initial board placements (`BoardGearPlacementData`: grid position + **`GearConfig`**) and optional starting inventory (`List<GearConfig>`). Hydration runs in **`GearEngineViewModel`** via **`BoardViewModel.LoadLayout`** and **`GearInventoryViewModel.LoadInventory`**.
* **`GearTestSceneBootstrap`**: Thin scene `MonoBehaviour` with serialized **`GearEngineStartData`**; after DI, calls **`INavigation`** to open **`GearEngineViewModel`**. Does not spawn nodes or gear views.
* **`BoardView`**: Standalone **`MonoBehaviour`** (not `ViewComponent`). Call **`Bind(BoardViewModel, interactable)`** / **`Unbind()`**. Use **`interactable: true`** for full drag/drop (enables sibling **`GearBoardDragHandler`**); use **`false`** for read-only or background boards. Inventory bridging (inventory slot → board, board → inventory over UI) is implemented in **`GearEngineView.OnBind`**, not on the board.
* **`GearEngineView`**: Parent **`View<GearEngineViewModel>`**; sub-widgets have **no `[Inject]`** fields. **`GearInventoryView`** builds slots without **`IObjectResolver`**. **`GearInventorySlotView`** notifies **`GearInventoryViewModel.NotifyGearDropped`**, which raises **`OnGearDraggedToBoard`** for the screen to call **`BoardViewModel.HandleInventoryDrop`** and **`ConsumeSpecificGear`** on success.
* **`GearInventoryLoadoutSO`**: Still used by editor tooling to populate test **`GearEngineStartData`** inventory when regenerating scenes; it is **not** registered with the mechanics installer.

**Scenes**

* [`Assets/Scenes/GearEngine_TestScene.unity`](../../../../Scenes/GearEngine_TestScene.unity) — full test layout (`TestCanvas`).
* [`Assets/Scenes/Gear_Clean.unity`](../../../../Scenes/Gear_Clean.unity) — same composable wiring with root `Canvas` (for merging with other UI).

**Editor menu**

* **GearEngine → Step 1: Generate Basic Setup Configs** — creates board, gear, tag, ability assets and **`GearInventoryLoadout.asset`**.
* **GearEngine → Generate Navigation Assets** — creates **`GearEngineView_NavigationStub.prefab`**, **`GearEngineViewConfig.asset`**, and registers the config on **`Assets/Data/Navigation/Navigation Settings.asset`** (required for `INavigation` context views).
* **GearEngine → Step 2: Generate VContainer Test Scene** — regenerates `GearEngine_TestScene.unity` (runs navigation generation first).
* **GearEngine → Create Gear_Clean Scene** — overwrites `Gear_Clean.unity` with the composable hierarchy.

To merge with another mechanic in one scene, add a parent `LifetimeScope` (or child scope), duplicate the `GearEngine_Root` subtree, assign **`navigationSettings`**, **`navigationViewHolder`** (root that parents the canvas with `GearEngineView`), **`boardConfig`**, and optional **`sceneBootstrap`** (or open **`GearEngineViewModel(startData)`** from your own flow).

---

## Test Suite (`GearEngineFlowTests.cs`)

The engine boasts an extensive NUnit test suite simulating the architecture in headless isolation (without Unity components overhead). It ensures that mathematical bounds and business logic never break.

### Key Covered Workflows
1.  **Core Ticking & Triggers**:
    *   [Test_CoreGear_Rotates_And_Fires_Trigger_To_Neighbor](Tests/Test_CoreGear_Rotates_And_Fires_Trigger_To_Neighbor.md): Ensures that when a `CoreGearNode` is ticked by the engine, it completes a segment and correctly dispatches `DirectionalTriggerEvent` precisely to its connected coordinate.
2.  **Base Gear Charging**:
    *   [Test_BaseGear_Executes_Abilities_When_Fully_Charged](Tests/Test_BaseGear_Executes_Abilities_When_Fully_Charged.md): Verifies that `BaseGearNode` safely accumulates charge from triggers and naturally resets charge back to 0 exactly when abilities are executed.
3.  **Aura System**:
    *   [Test_AuraGear_Boosts_Neighbors_And_Core_Speed](Tests/Test_AuraGear_Boosts_Neighbors_And_Core_Speed.md): Proves the GridManager correctly calculates and applies the Aura scaling to a neighbor node *before* the neighbor ticks its own rotation calculation logic.
4.  **Merge Logic**:
    *   [Test_GearMergeService_Merges_Identical_Gears](Tests/Test_GearMergeService_Merges_Identical_Gears.md): Confirms `GearMergeService` resolves two identical level 1 gears into a valid level 2 gear state, consuming the correct configuration properties.
5.  **State Pausing & Wind-Down**:
    *   [Test_GridManager_Stop_Triggers_WindDown](Tests/Test_GridManager_Stop_Triggers_WindDown.md): Asserts that when the engine calls `Stop()`, standard `Tick` logic freezes and gears smoothly revert their rotation angles via the wind-down lerping method.
6.  **Destructible Hazards**:
    *   [Test_Obstacle_Destroys_Self_On_Max_Charge](Tests/Test_Obstacle_Destroys_Self_On_Max_Charge.md): Ensures that a non-interactable `BaseGearNode` (Obstacle) properly absorbs enough triggers to fire its `DestroySelfAbility`, accurately emitting a `GearDestroyedEvent` once broken.
7.  **Runtime Abilities**:
    *   [Test_RuntimeAbility_Deactivates_Node_Temporarily](Tests/Test_RuntimeAbility_Deactivates_Node_Temporarily.md): Verifies that temporary status effects successfully hijack node logic and expire correctly.
