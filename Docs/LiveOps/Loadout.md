# Loadout module (LiveOps)

## TL;DR

- **DTO**: `LoadoutPersistence` / `LoadoutPlacement` (`gearId`, `x`, `y`), `LoadoutGameData`, `SaveBoardLayoutRequest` / `SaveBoardLayoutResponse`, `ClearBoardRequest` / `ClearBoardResponse` (all mutations `[UsesGameApi]`).
- **Cloud Code**: `LoadoutModule` (`Initialize`); `SaveBoardLayoutHandler`, `ClearBoardHandler`.
- **Unity**: `LoadoutClientModule` implements `IGearLoadoutService` (board only). Resolves `GearConfig` via `GearCatalogSO`.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
