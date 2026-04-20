# Roguelike module (LiveOps)

## TL;DR

- **DTO**: `RoguelikeConfig` (`gearPool`, `optionsPerRoll`), `RoguelikePersistence` (`currentRollIds`), `RoguelikeGameData`, `DrawRoguelikeRollRequest` / `DrawRoguelikeRollResponse`, `ClaimRoguelikePickRequest` / `ClaimRoguelikePickResponse` (`[UsesGameApi]`).
- **Cloud Code**: `RoguelikeModule` (`Initialize`); `DrawRoguelikeRollHandler` returns an existing roll if one is already persisted, otherwise picks `optionsPerRoll` ids from `gearPool` via `IRoguelikeSelectionStrategy` (default `RandomRoguelikeSelectionStrategy`); `ClaimRoguelikePickHandler` validates the pick against the current roll and clears persistence.
- **Unity**: `RoguelikeClientModule` in [`Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/LiveOps/`](../../Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/LiveOps/); registered via `CampaignRoguelikeInstaller` in `LiveOpsClientModulesLayer`; `IRoguelikeRollService` / `RoguelikeRollService` maps roll ids to `GearConfig` via `GearCatalogSO` (registered in `CampaignLayer`).
- **Remote Config**: [Roguelike.rc](../../Assets/LiveOps/RemoteConfig/Roguelike.rc).

## References

- [NewApiAndServices.md](NewApiAndServices.md)
- [RemoteConfig.md](RemoteConfig.md)
