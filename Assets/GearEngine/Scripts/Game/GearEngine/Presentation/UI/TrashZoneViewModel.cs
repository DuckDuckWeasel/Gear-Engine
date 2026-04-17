using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services.Inventory;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class TrashZoneViewModel : ViewModel
    {
        private readonly IDragService dragService;
        private readonly IGearEngineService engineService;
        private readonly BoardViewModel board;
        private readonly IInventoryService inventoryService;
        private readonly IEventBus eventBus;
        private readonly GearEngineFeatureToggleSO featureToggle;

        [ObservableProperty]
        private bool isActive;

        [ObservableProperty]
        private string rewardText = string.Empty;

        public TrashZoneViewModel(
            IDragService dragService,
            IGearEngineService engineService,
            BoardViewModel board,
            IInventoryService inventoryService,
            IEventBus eventBus,
            GearEngineFeatureToggleSO featureToggle)
        {
            this.dragService = dragService ?? throw new ArgumentNullException(nameof(dragService));
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.eventBus = eventBus;
            this.featureToggle = featureToggle;
        }

        public void RegisterAsDragTarget(IDragTarget target) => dragService?.Register(target);

        public void UnregisterAsDragTarget(IDragTarget target) => dragService?.Unregister(target);

        public bool CanTrashAcceptGear(GearConfigData gear)
        {
            if (gear == null || !gear.IsDeletable)
            {
                return false;
            }

            if (featureToggle != null && !featureToggle.EnableTrashDeletion)
            {
                return false;
            }

            if (engineService != null && engineService.IsRunning)
            {
                return false;
            }

            return true;
        }

        public void HandleDragStarted(object data)
        {
            if (data is GearConfigData gearData && CanTrashAcceptGear(gearData))
            {
                RewardText = $"+{gearData.DeleteRewardAmount}";
                IsActive = true;
            }
            else
            {
                IsActive = false;
            }
        }

        public void HandleDragEnded()
        {
            IsActive = false;
        }

        public void HandleBoardGearDropped(IGridNode node)
        {
            try
            {
                if (node == null)
                {
                    return;
                }

                if (engineService != null && engineService.IsRunning)
                {
                    return;
                }

                bool deleted = board.DeleteGear(node);
                if (!deleted)
                {
                    board.SnapBackToOriginal(node);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrashZoneViewModel] HandleBoardGearDropped failed: {ex.Message}\n{ex.StackTrace}");
                try
                {
                    board?.SnapBackToOriginal(node);
                }
                catch (Exception snapEx)
                {
                    Debug.LogError($"[TrashZoneViewModel] SnapBackToOriginal failed: {snapEx.Message}\n{snapEx.StackTrace}");
                }
            }
            finally
            {
                dragService?.EndDrag();
            }
        }

        public void HandleInventoryGearDropped(GearConfigData gear)
        {
            try
            {
                if (gear == null)
                {
                    return;
                }

                if (engineService != null && engineService.IsRunning)
                {
                    return;
                }

                inventoryService.ConsumeSpecificItem(gear);

                if (gear.DeleteRewardAmount > 0)
                {
                    board.GrantTrashReward(gear.DeleteRewardAmount);
                }

                Debug.Log($"<color=#ff5555>[TrashZoneViewModel]</color> Inventory Item '{gear.Id}' Trashed! Reward: {gear.DeleteRewardAmount}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrashZoneViewModel] HandleInventoryGearDropped failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                dragService?.EndDrag();
            }
        }
    }
}
