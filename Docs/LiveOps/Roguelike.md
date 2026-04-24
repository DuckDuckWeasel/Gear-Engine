# Roguelike module (LiveOps)

## TL;DR

- **DTO**: `RoguelikeConfig` (`gearPool`, `optionsPerRoll`), `RoguelikePersistence` (`currentRollIds`), `RoguelikeGameData`, `DrawRoguelikeRollRequest` / `DrawRoguelikeRollResponse`, `ClaimRoguelikePickRequest` / `ClaimRoguelikePickResponse` (`[UsesGameApi]`).
- **Cloud Code**: `RoguelikeModule` (`Initialize`); `DrawRoguelikeRollHandler` returns an existing roll if one is already persisted, otherwise picks `optionsPerRoll` ids from `gearPool` via `IRoguelikeSelectionStrategy` (default `RandomRoguelikeSelectionStrategy`); `ClaimRoguelikePickHandler` validates the pick against the current roll and clears persistence.
- **Unity**: `RoguelikeClientModule` in [`Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/LiveOps/`](../../Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/LiveOps/); registered via `CampaignRoguelikeInstaller` inside [`CampaignLayer`](../../Assets/GearEngine/Scripts/App/Bootstrap/Layers/CampaignLayer.cs); `IRoguelikeRollService` / `RoguelikeRollService` maps roll ids to `GearConfig` via `GearCatalogSO`, published into the Campaign scope from **`FoundationLayer`** by a rebaked [`AddressableScriptableObjectPublisherSO`](../../Assets/GearEngine/Scripts/App/Bootstrap/Publishers/DataDriven/AddressableScriptableObjectPublisherSO.cs) targeting the Gear catalog ([`AddressableCatalogAddresses.Gear`](../../Assets/GearEngine/Scripts/App/Bootstrap/AddressableCatalogAddresses.cs)). The Addressable `RoguelikeGearPoolSO` is published the same way (`Catalogs/RoguelikeGearPool`).
- **Remote Config**: [Roguelike.rc](../../Assets/LiveOps/RemoteConfig/Roguelike.rc).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [RemoteConfig.md](RemoteConfig.md)
