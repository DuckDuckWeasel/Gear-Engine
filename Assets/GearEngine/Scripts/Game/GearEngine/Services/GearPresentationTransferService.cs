using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Events;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    public sealed class GearPresentationTransferService : IGearPresentationTransferService
    {
        public GearPresentationTransferService(IInventoryService inventoryService, IEventBus eventBus)
        {
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.eventBus = eventBus;
        }

        private readonly IInventoryService inventoryService;
        private readonly IEventBus eventBus;

        public void TrashInventoryGear(GearItemData gear)
        {
            if (gear == null)
            {
                return;
            }

            try
            {
                ApplyInventoryTrash(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearPresentationTransferService] TrashInventoryGear failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ApplyInventoryTrash(GearItemData gear)
        {
            if (gear.Owner == null)
            {
                Debug.LogWarning("[GearPresentationTransferService] TrashInventoryGear: gear has no Owner.");
                return;
            }

            inventoryService.Remove(gear.Owner);
            RaiseTrashReward(gear.DeleteRewardAmount);
        }

        private void RaiseTrashReward(int amount)
        {
            if (amount > 0)
            {
                eventBus?.Raise(new GearDeletedEvent(Vector2Int.zero, amount));
            }
        }
    }
}
