using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    public class GearTrashService : IGearTrashService
    {
        private readonly IGridManager gridManager;
        private readonly IGearEngineService engineService;
        private readonly IDragService dragService;
        
        public BoardViewModel LinkedBoard { get; set; }
        public Inventory.IInventoryService LinkedInventory { get; set; }

        public GearTrashService(IGridManager gridManager, IGearEngineService engineService, IDragService dragService)
        {
            this.gridManager = gridManager;
            this.engineService = engineService;
            this.dragService = dragService;
        }

        public void RequestTrashDrop(IGridNode node)
        {
            if (LinkedBoard == null || engineService == null)
            {
                Debug.LogWarning("[GearTrashService] RequestTrashDrop skipped: LinkedBoard or engineService is null.");
                return;
            }

            try
            {
                bool deleted = LinkedBoard.DeleteGear(node);

                if (!deleted)
                {
                    Debug.Log($"<color=#ff9900>[GearTrashService]</color> DeleteGear returned false. Snapping back.");
                    LinkedBoard.SnapBackToOriginal(node);
                }
                else
                {
                    Debug.Log($"<color=#00ff88>[GearTrashService]</color> Board gear successfully trashed!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTrashService] RequestTrashDrop failed: {ex.Message}\n{ex.StackTrace}");
                LinkedBoard.SnapBackToOriginal(node);
            }
            finally
            {
                // Always end the drag after a trash drop attempt (board gears)
                dragService?.EndDrag();
            }
        }

        public void HandleInventoryGearDropped(GearConfigData gearData)
        {
            if (LinkedInventory == null || engineService == null)
            {
                Debug.LogWarning("[GearTrashService] HandleInventoryGearDropped skipped: LinkedInventory or engineService is null.");
                return;
            }

            if (engineService.IsRunning)
            {
                return;
            }

            if (gearData == null)
            {
                return;
            }

            try
            {
                LinkedInventory.ConsumeSpecificItem(gearData);

                // Grant the scrap reward for deleting the gear
                if (LinkedBoard != null && gearData.DeleteRewardAmount > 0)
                {
                    LinkedBoard.GrantTrashReward(gearData.DeleteRewardAmount);
                }

                Debug.Log($"<color=#ff5555>[GearTrashService]</color> Inventory Item '{gearData.Id}' Trashed! Reward: {gearData.DeleteRewardAmount}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTrashService] HandleInventoryGearDropped failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
