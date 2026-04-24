# Inventory module (LiveOps) — owned gear instances

## TL;DR

- **DTO**: `InventoryPersistence` (`startingGearsSeeded` one-shot guard) / `InventoryGameData` (`gears`: list of `{ instanceId, gearId }`, `motorCogGearId` derived from **`InventoryConfig.StartingGearIds[0]`** for client/UI), `SetInventoryRequest` / `SetInventoryResponse` (`[UsesGameApi]`). `instanceId` is a client-minted GUID (string) except the seeded core gear uses the fixed id **`motor`** and other starter gears use `start_<guid>`; `gearId` is the catalog key.
- **Cloud Code**: `InventoryModule` (`Initialize` — if `InventoryPersistence.StartingGearsSeeded == false`, appends one entry per ordered `InventoryConfig.StartingGearIds`: index `0` uses `instanceId: "motor"`, others `start_<guid>`, then sets the flag and writes **`Player.Set`**); `SetInventoryHandler` (stores the list, dedupes by `instanceId`).
- **Unity**: `InventoryClientModule` implements **`IInventoryService`** (`Owned` as `IReadOnlyList<OwnedGear>`, `MotorCogGearId`, `Add`, `Remove`, `Clear`, `InventoryChanged`). Default motor **grid cell** when the loadout has no core placement comes from **`LoadoutConfig`** (see Loadout module), not inventory. Mutations persist via fire-and-forget `SetInventoryRequest` (full snapshot). Author **`InventoryConfigBuilderSO`**: **`motorCogGear`** (`GearConfig` SO) plus **`additionalStartingGears`** (more `GearConfig` refs); `Build()` merges their ids into remote `startingGearIds` (motor first). Optional **`gearCatalogForApply`** resolves ids when using Apply (pull/sync).

## Starter inventory

`InventoryConfig.StartingGearIds` is a single ordered list: **index `0` must be the core/motor catalog id** (same role as the former `motorCogGearId`). Brand-new players receive every id in that list once, on the first `InventoryModule.Initialize`. The seed is gated by `InventoryPersistence.StartingGearsSeeded`, so removing a starter gear does not cause it to respawn on later sessions. Starter gears live in the inventory tray—they are not auto-placed on the board. Existing accounts created before a config change follow the same one-shot rule for the combined list.

## Editor cheats (Play mode)

Use **Gear Engine > Cheats > Inventory** in the Unity Editor while the campaign is running in Play mode: add a random gear, add one of each entry in `GearCatalogSO`, or clear the inventory. These resolve `IInventoryService` / `GearCatalogSO` from the live `ApplicationBootstrap` scope.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [GearEngine.md](../Game/GearEngine.md) (board vs inventory boundaries)
- [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md)
