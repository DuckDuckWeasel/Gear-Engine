# Gear Engine (Game.GearEngine)

## Ownership and initialization

- **`IInventoryService` / `InventoryService`** — Owns a single **`InventoryModel`**. Seeded once from **`GearInventoryLoadoutData`** when `GearMechanicsInstaller.Install(...)` runs. Consumers use `GetInventory()`, `TryAdd`, and `TryConsume`; view models do not initialize inventory.
- **`IBoardService` / `BoardService`** — Owns **`BoardModel`** (board config reference, simulation flag, and a synced node collection) and all grid mutations (`TryPlace`, `TryMoveBoardGear`, `TryRemoveBoardGear`, `TryDeleteBoardGear`, `LoadLayout`). Seeded from **`GearBoardLoadoutData`** at installer time. `BoardViewModel` binds and forwards UI/drag flow; return-to-inventory still coordinates `IInventoryService` + board removal in the view model.
- **`GearEngineStartData`** — Adapter exposing `GetInventoryLoadoutData()` and `GetBoardLoadoutData()` for scopes without forcing a single god-object at all call sites.

## Scopes

`CampaignScope`, `RaceScope`, and `GearMechanicsScope` pass inventory and board loadout data into `GearMechanicsInstaller` so services are constructed before any gear UI view model runs.
