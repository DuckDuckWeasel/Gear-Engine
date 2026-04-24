# Loadout module (LiveOps)

## TL;DR

- **DTO**: `LoadoutPersistence` / `LoadoutPlacement` (`instanceId`, `gearId`, `x`, `y`), `LoadoutGameData`, `LoadoutConfig` (`baseSlots`, `motorCogStartX` / `motorCogStartY` for default core placement), `SaveBoardLayoutRequest` / `SaveBoardLayoutResponse` (`rejected`, `reason`, `savedAtUtcTicks`), `ClearBoardRequest` / `ClearBoardResponse` (all mutations `[UsesGameApi]`). `instanceId` ties each cell to a specific **inventory instance**; `gearId` is a denormalized hint.
- **Cloud Code**: `LoadoutModule` (`Initialize`); `SaveBoardLayoutHandler` (filters placements to those whose `instanceId` exists in `InventoryPersistence.Gears`; **rejects** with `rejected: true`, `reason: "missing_motor_cog"` if no placement’s `gearId` matches the core id from `InventoryConfig.StartingGearIds[0]` when that id is non-empty), `ClearBoardHandler`.
- **Unity**: `LoadoutClientModule` implements `IGearLoadoutService` (board only). `GetBoardLayout()` resolves each placement’s `OwnedGear` from `IInventoryService.Owned` by `instanceId` and **ensures** the motor cog is present at the cell from **`LoadoutGameData`** / remote `LoadoutConfig` when it is missing from the saved board. Author **`BoardRulesSO.MotorCogStartCell`** to match that cell for grid authoring consistency (Meta bootstrap has no board rules in DI; placement uses loadout config). `PersistBoardLayout` serializes `BoardGearPlacementData.Owner.InstanceId` (and `gearId` for diagnostics); if the server rejects the save, the module restores the previous in-memory `Board` snapshot and logs an error.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [Inventory.md](Inventory.md)
