# Inventory module (LiveOps) — owned gear instances

## TL;DR

- **DTO**: `InventoryPersistence` (adds `startingGearsSeeded` one-shot guard) / `InventoryGameData` (`gears`: list of `{ instanceId, gearId }`), optional `motorCogGearId` / `motorCogStartX` / `motorCogStartY` / `startingGearIds` (from **`InventoryConfig`** / remote config), `SetInventoryRequest` / `SetInventoryResponse` (`[UsesGameApi]`). `instanceId` is a client-minted GUID (string) except the seeded motor cog uses the fixed id **`motor`** and seeded starter gears use `start_<guid>`; `gearId` is the catalog key.
- **Cloud Code**: `InventoryModule` (`Initialize` — if no owned row matches `InventoryConfig.MotorCogGearId`, inserts `{ instanceId: "motor", gearId }`; if `InventoryPersistence.StartingGearsSeeded == false`, appends one entry per `InventoryConfig.StartingGearIds` (skipping null/empty), sets the flag, and writes a single **`Player.Set`** for both seeds); `SetInventoryHandler` (stores the list, dedupes by `instanceId`).
- **Unity**: `InventoryClientModule` implements **`IInventoryService`** (`Owned` as `IReadOnlyList<OwnedGear>`, `MotorCogGearId`, `MotorCogStartCell`, `Add`, `Remove`, `Clear`, `InventoryChanged`). Each `OwnedGear` holds `InstanceId` + `GearConfig`. Mutations persist via fire-and-forget `SetInventoryRequest` (full snapshot); ordering is handled by the Cloud Code single-flight-per-module policy. The gear-engine **tray** is a derived view (`Owned` minus placed board gears, matched by `OwnedGear` reference on `GearConfigData.Owner`); there is no client-side inventory slot cap—only **`BoardRulesSO.MaxAllowedBoardGears`** limits placements. Author **`InventoryConfigBuilderSO`** for default motor id, start cell, and starter gear ids (pushed as remote config).

## Starter inventory

Brand-new players receive the motor cog plus every entry in `InventoryConfig.StartingGearIds` once, on their first `InventoryModule.Initialize`. The seed is gated by `InventoryPersistence.StartingGearsSeeded`, so removing a starter gear (selling, scrapping, etc.) does not cause it to respawn on later sessions. Starter gears live in the inventory tray—they are not auto-placed on the board. Existing accounts created before this field shipped will be treated as new players for seeding purposes (they get the gears once on next login); to opt them out, push a config with an empty `startingGearIds` list before they reconnect.

## Editor cheats (Play mode)

Use **Gear Engine > Cheats > Inventory** in the Unity Editor while the campaign is running in Play mode: add a random gear, add one of each entry in `GearCatalogSO`, or clear the inventory. These resolve `IInventoryService` / `GearCatalogSO` from the live `ApplicationBootstrap` scope.

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [GearEngine.md](../Game/GearEngine.md) (board vs inventory boundaries)
- [State-and-Services-Standard.md](../Standards/State-and-Services-Standard.md)
