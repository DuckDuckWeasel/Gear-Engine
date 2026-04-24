# Roguelike gear pick (Campaign)

## TL;DR

- **UI**: [`RoguelikeView`](../../Assets/GearEngine/Scripts/Game/Campaign/Presentation/RoguelikeView.cs) / [`RoguelikeViewModel`](../../Assets/GearEngine/Scripts/Game/Campaign/Presentation/RoguelikeViewModel.cs) with [`CampaignRoguelikeViewConfig`](../../Assets/GearEngine/Data/Campaign/ViewConfigs/CampaignRoguelikeViewConfig.asset) and prefab [`Campaign_RoguelikeView`](../../Assets/GearEngine/Prefabs/Campaign/Campaign_RoguelikeView.prefab).
- **Roll**: Options come from `IRoguelikeRollService`, backed by LiveOps `RoguelikeClientModule` (authoritative roll persisted server-side so quitting mid-selection does not re-roll).
- **Confirm**: Adds the chosen gear via **`IInventoryService.TryAdd(selectedCard.GearConfig)`**, which updates owned inventory and persists with `SetInventoryRequest`. Then `ConsumePickAsync` clears the server roll via `ClaimRoguelikePickRequest`. **`CampaignGearPersistenceHookup`** does not mirror inventory from a race service—it only persists **loadout** when the board layout changes.
- **Capacity**: **`CanConfirm`** is **`selectedCard != null`**. There is no inventory cap on pickup; board placement cap follows LiveOps loadout **`baseSlots`** (see **`IBoardSlotCapacityProvider`** / **`BoardService.MaxAllowedBoardGears`**).

## References

- [Roguelike module (LiveOps)](../LiveOps/Roguelike.md)
- [Campaign.md](Campaign.md)
- [Inventory.md](../LiveOps/Inventory.md)
