# Inventory module (LiveOps) — owned gear ids

## TL;DR

- **DTO**: `InventoryPersistence` / `InventoryGameData` (`gearIds`), `SetInventoryRequest` / `SetInventoryResponse` (`[UsesGameApi]`).
- **Cloud Code**: `InventoryModule` (`Initialize`); `SetInventoryHandler`.
- **Unity**: `InventoryClientModule` implements `IOwnedGearInventoryService`. In-race grid inventory remains `IRaceInventoryService` / `InventoryService`.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
