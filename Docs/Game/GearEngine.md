# Gear Engine (Game.GearEngine)

## Ownership and initialization

- **`IInventoryService` / `InventoryService`** — Owns a single **`InventoryModel`**. Seeded once from **`GearInventoryLoadoutData`** when `GearMechanicsInstaller.Install(...)` runs. Consumers use `GetInventory()`, `TryAdd`, and `TryConsume`; view models do not initialize inventory.
- **`IBoardService` / `BoardService`** — Owns **`BoardModel`** (board config reference, simulation flag, and a synced node collection) and all grid mutations (`TryPlace`, `TryMoveBoardGear`, `TryRemoveBoardGear`, `TryDeleteBoardGear`, `LoadLayout`). Seeded from **`GearBoardLoadoutData`** at installer time. `BoardViewModel` binds and forwards UI/drag flow; return-to-inventory still coordinates `IInventoryService` + board removal in the view model.
- **`GearEngineStartData`** — Adapter exposing `GetInventoryLoadoutData()` and `GetBoardLoadoutData()` for scopes without forcing a single god-object at all call sites.

## Scopes

`CampaignScope`, `RaceScope`, and `GearMechanicsScope` pass inventory and board loadout data into `GearMechanicsInstaller` so services are constructed before any gear UI view model runs.

## Drag presentation

- **`DragGhostController`** — Instantiates the **`GearVisual`** child from `GearConfigData.ViewPrefab` under the board’s space root (via `BoardViewComponent.GetBoardSpaceRoot()`), applies `RelativeScaleMultiplier` as local scale, and moves the ghost in world space. Used for both inventory drags (`GearInventoryViewComponent`) and board drags (`GearBoardDragHandler`) so the ghost matches placed gears without canvas/world scale ratio math.
- **`DragHandler`** — EventSystem forwarder only: `OnDragBegin` / `OnDragMoved` / `OnDragEnd` with `PointerEventData`, plus `BuildPayload` and drop resolution via `DragTargetFinder`. Inventory slots wire these callbacks to `DragGhostController`; slot and ghost visuals use `GearView.BindForDisplay` with the authored `GearConfigData.ViewPrefab`.
