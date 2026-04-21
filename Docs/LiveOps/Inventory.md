# Inventory module (LiveOps) — owned gear ids

## TL;DR

- **DTO**: `InventoryPersistence` / `InventoryGameData` (`gearIds`), `SetInventoryRequest` / `SetInventoryResponse` (`[UsesGameApi]`).
- **Cloud Code**: `InventoryModule` (`Initialize`); `SetInventoryHandler`.
- **Unity**: `InventoryClientModule` implements **`IInventoryService`** (`Owned`, `TryAdd`, `TryRemove`, `Clear`, `InventoryChanged`). Mutations persist via `SetInventoryRequest` in the background. The gear-engine **tray** is a derived view (`Owned` minus placed board gears); there is no client-side inventory slot cap—only **`BoardRulesSO.MaxAllowedBoardGears`** limits placements.

## Editor cheats (Play mode)

Use **Gear Engine > Cheats > Inventory** in the Unity Editor while the campaign is running in Play mode: add a random gear, add one of each entry in `GearCatalogSO`, or clear the inventory. These resolve `IInventoryService` / `GearCatalogSO` from the live `ApplicationBootstrap` scope.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [GearEngine.md](../Game/GearEngine.md) (board vs inventory boundaries)
- [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md)
