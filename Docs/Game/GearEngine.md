# Gear Engine (Game.GearEngine)

## Ownership and initialization

- **`IInventoryService`** — Persisted, server-authoritative list of owned gears (`Owned`, `TryAdd`, `TryRemove`, `InventoryChanged`). In Campaign this is **`InventoryClientModule`** (LiveOps). In race/test scopes without LiveOps, register **`EmptyInventoryService`**. **`GearMechanicsInstaller` does not register** `IInventoryService`; the host scope must register it **before** the installer runs.
- **`IBoardService` / `BoardService`** — Owns **`BoardModel`** (board config reference, simulation flag, and a synced node collection) and all grid mutations (`TryPlace`, `TryMoveBoardGear`, `TryRemoveBoardGear`, `TryDeleteBoardGear`, `LoadLayout`). There is no construction-time seed: **`SetupViewModel`** (Campaign) calls **`LoadLayout`** from **`IGearLoadoutService.GetBoardLayout()`**; **`GearTestSceneBootstrap`** (Gear test scene) calls **`LoadLayout`** from its serialized **`boardSeed`**; **`BoardViewModel.LoadLayout`** is driven from **`RaceStartData.GearEngineData.BoardLayout`** in the race flow. **`TryDeleteBoardGear`** also removes the gear from **`IInventoryService`** (destroyed, not returned). Max placed gears (`TryPlace`) is capped by **`IBoardSlotCapacityProvider.BoardSlotCapacity`** when that value is positive (LiveOps **`LoadoutGameData.BaseSlots`** via **`LoadoutClientModule`**), otherwise by the grid size (**`BoardRulesSO.MaxBoardGears`**).
- **`IBoardSlotCapacityProvider`** — Implemented by **`LoadoutClientModule`** (Campaign) and **`UnlimitedBoardSlotCapacityProvider`** (sandbox **`GearMechanicsScope`** / **`RaceScope`**). Registered before **`GearMechanicsInstaller`**.
- **`IGearLoadoutService`** — Persisted board layout `(gearId, x, y)` plus **`BoardSlotCapacity`** (loadout `baseSlots`); **`LoadoutClientModule`** in Campaign. **`CampaignGearPersistenceHookup`** forwards **`BoardLayoutChanged`** to persist layout only (no inventory bridging).
- **`GearEngineStartData`** — Serializable payload with **`BoardLayoutData`** (e.g. **`RaceStartData.GearEngineData`**). Initial tray contents come from **`IInventoryService.Owned`**, not start data.

## Tray (derived UI)

- **`GearInventoryViewModel`** exposes **`TrayItems`**: multiset **`Owned` minus gears currently on the board** (from **`IBoardService.GetAllNodes()`**). It subscribes to **`InventoryChanged`**, **`GearPlaced`**, and **`GearRemoved`**. There is no separate race inventory or `InventoryModel`.

## Scopes

`CampaignLayer` registers inventory + loadout via LiveOps installers, then registers **`IBoardSlotCapacityProvider`** (via **`LoadoutClientModule`**) before `GearMechanicsInstaller`. `RaceScope` and `GearMechanicsScope` register **`EmptyInventoryService`** and **`UnlimitedBoardSlotCapacityProvider`** before the installer.

## Workspace presentation ownership

- **`GearWorkspaceView`** is a canvas-less UI composition owned by each campaign screen prefab. Setup and Roguelike bind Board, Inventory, Trash, and the drag overlay in interactive mode. Active Race binds only the Board in read-only mode.
- **`Main Scene`** owns navigation and world presentation only. It no longer contains shared Board, Inventory, or Trash objects and does not inject scene instances into campaign screen prefabs.
- Track, cars, environment, and race cameras remain world-space. Gear workspace visuals are `Image` and `RectTransform` components rendered by the screen's existing Canvas and `GraphicRaycaster`.
- **`SafeAreaRectTransform`** keeps the workspace inside the current device safe area. The reference composition is 1080x1920 portrait; `BoardLayoutSO` presentation dimensions are pixels at that reference resolution.

## Input and drag flow

- **`DragPayload.ScreenPosition`** carries the pointer position without any camera projection or world plane.
- **`Draggable.Configure(IDragService, RectTransform)`** receives the screen-owned drag service and workspace overlay explicitly. A drag preview is instantiated beneath that overlay and positioned with `RectTransformUtility.ScreenPointToLocalPointInRectangle` using the owning Canvas event camera.
- **`DragTargetFinder`** accepts targets returned by EventSystem UI raycasts only. Gear interaction does not require Colliders, `PhysicsRaycaster`, `Physics2DRaycaster`, screen-to-world planes, or camera-distance fitting.
- **`BoardScreenPositionUtility`** converts a screen pointer into Board-local pixels. `BoardLayoutSO` then maps those pixels to logical rows and columns, including staggered-row offsets. Domain coordinates, placement rules, saves, and LiveOps data remain unchanged.
- A Gear view creates an isolated instance of its charge-fill material. This prevents charge updates on one Gear from changing another Gear's UI material state.

## Standard

See [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md) for Tier 1/2 boundaries; inventory vs loadout vs session board state follows the two-service model above.
