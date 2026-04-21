# Inventory module (LiveOps) — owned gear instances

## TL;DR

- **DTO**: `InventoryPersistence` / `InventoryGameData` (`gears`: list of `{ instanceId, gearId }`), optional `motorCogGearId` / `motorCogStartX` / `motorCogStartY` (from **`InventoryConfig`** / remote config), `SetInventoryRequest` / `SetInventoryResponse` (`[UsesGameApi]`). `instanceId` is a client-minted GUID (string) except the seeded motor cog uses the fixed id **`motor`**; `gearId` is the catalog key.
- **Cloud Code**: `InventoryModule` (`Initialize` — if no owned row matches `InventoryConfig.MotorCogGearId`, inserts `{ instanceId: "motor", gearId }` and **`Player.Set`** persistence); `SetInventoryHandler` (stores the list, dedupes by `instanceId`).
- **Unity**: `InventoryClientModule` implements **`IInventoryService`** (`Owned` as `IReadOnlyList<OwnedGear>`, `MotorCogGearId`, `MotorCogStartCell`, `Add`, `Remove`, `Clear`, `InventoryChanged`). Each `OwnedGear` holds `InstanceId` + `GearConfig`. Mutations persist via fire-and-forget `SetInventoryRequest` (full snapshot); ordering is handled by the Cloud Code single-flight-per-module policy. The gear-engine **tray** is a derived view (`Owned` minus placed board gears, matched by `OwnedGear` reference on `GearConfigData.Owner`); there is no client-side inventory slot cap—only **`BoardRulesSO.MaxAllowedBoardGears`** limits placements. Author **`InventoryConfigBuilderSO`** for default motor id and start cell (pushed as remote config).

## Editor cheats (Play mode)

Use **Gear Engine > Cheats > Inventory** in the Unity Editor while the campaign is running in Play mode: add a random gear, add one of each entry in `GearCatalogSO`, or clear the inventory. These resolve `IInventoryService` / `GearCatalogSO` from the live `ApplicationBootstrap` scope.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [GearEngine.md](../Game/GearEngine.md) (board vs inventory boundaries)
- [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md)
