# Roguelike gear pick (Campaign)

## TL;DR

- **UI**: [`RoguelikeView`](../../Assets/GearEngine/Scripts/Game/Campaign/Presentation/RoguelikeView.cs) / [`RoguelikeViewModel`](../../Assets/GearEngine/Scripts/Game/Campaign/Presentation/RoguelikeViewModel.cs) with [`CampaignRoguelikeViewConfig`](../../Assets/GearEngine/Data/Campaign/CampaignRoguelikeViewConfig.asset) and prefab [`Campaign_RoguelikeView`](../../Assets/GearEngine/Prefabs/Campaign/Campaign_RoguelikeView.prefab).
- **Roll**: Options come from `IRoguelikeRollService`, backed by LiveOps `RoguelikeClientModule` (authoritative roll persisted server-side so quitting mid-selection does not re-roll).
- **Confirm**: Adds the chosen gear via `IRaceInventoryService.TryAdd`; [`CampaignGearPersistenceHookup`](../../Assets/GearEngine/Scripts/Game/Campaign/Bootstrap/CampaignGearPersistenceHookup.cs) mirrors race inventory to owned inventory (`InventoryClientModule`). Then `ConsumePickAsync` clears the server roll via `ClaimRoguelikePickRequest`.
- **Capacity**: `CanConfirm` is false when the race inventory tray is full; the player can use the trash zone to free a slot before confirming.

## References

- [Roguelike module (LiveOps)](../LiveOps/Roguelike.md)
- [Campaign.md](Campaign.md)
