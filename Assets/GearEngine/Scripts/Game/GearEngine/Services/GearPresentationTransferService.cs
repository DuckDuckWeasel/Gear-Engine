using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
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

        public void AddReturnedBoardGearToInventory(GearConfigData config)
        {
            if (config == null)
            {
                return;
            }

            try
            {
                inventoryService.AddItem(config);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearPresentationTransferService] AddReturnedBoardGearToInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

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
            inventoryService.ConsumeSpecificItem(gear);
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
