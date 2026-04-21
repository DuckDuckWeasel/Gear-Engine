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

        public void TrashInventoryGear(GearConfigData gear)
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

        private void ApplyInventoryTrash(GearConfigData gear)
        {
            GearConfig source = gear.SourceGearConfig;
            if (source == null)
            {
                Debug.LogWarning("[GearPresentationTransferService] TrashInventoryGear: gear has no SourceGearConfig.");
                return;
            }

            inventoryService.TryRemove(source);
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
