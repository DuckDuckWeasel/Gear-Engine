# Loadout module (LiveOps)

## TL;DR

- **DTO**: `LoadoutPersistence` / `LoadoutPlacement` (`instanceId`, `gearId`, `x`, `y`), `LoadoutGameData`, `SaveBoardLayoutRequest` / `SaveBoardLayoutResponse`, `ClearBoardRequest` / `ClearBoardResponse` (all mutations `[UsesGameApi]`). `instanceId` ties each cell to a specific **inventory instance**; `gearId` is a denormalized hint.
- **Cloud Code**: `LoadoutModule` (`Initialize`); `SaveBoardLayoutHandler` (filters placements to those whose `instanceId` exists in `InventoryPersistence.Gears`), `ClearBoardHandler`.
- **Unity**: `LoadoutClientModule` implements `IGearLoadoutService` (board only). `GetBoardLayout()` resolves each placement’s `OwnedGear` from `IInventoryService.Owned` by `instanceId`. `PersistBoardLayout` serializes `BoardGearPlacementData.Owner.InstanceId` (and `gearId` for diagnostics).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [Inventory.md](Inventory.md)
