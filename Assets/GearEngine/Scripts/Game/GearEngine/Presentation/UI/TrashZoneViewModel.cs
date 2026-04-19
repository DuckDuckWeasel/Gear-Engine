using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class TrashZoneViewModel : ViewModel
    {
        public TrashZoneViewModel(IDragService dragService, IGearEngineService engineService, BoardViewModel board, IGearPresentationTransferService presentationTransfer, GearEngineFeatureToggleSO featureToggle)
        {
            this.dragService = dragService ?? throw new ArgumentNullException(nameof(dragService));
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.presentationTransfer = presentationTransfer ?? throw new ArgumentNullException(nameof(presentationTransfer));
            this.featureToggle = featureToggle;
        }

        internal GearEngineFeatureToggleSO FeatureToggleForTrashPlacement => featureToggle;

        private readonly IDragService dragService;
        private readonly IGearEngineService engineService;
        private readonly BoardViewModel board;
        private readonly IGearPresentationTransferService presentationTransfer;
        private readonly GearEngineFeatureToggleSO featureToggle;

        [ObservableProperty]
        private bool isActive;

        [ObservableProperty]
        private string rewardText = string.Empty;

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
                TryDeleteBoardGearOrSnapBack(node);
            }
            catch (Exception ex)
            {
                LogBoardDropFailure(ex, node);
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
                TryTrashInventoryGear(gear);
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

        private void TryDeleteBoardGearOrSnapBack(IGridNode node)
        {
            if (node == null || (engineService != null && engineService.IsRunning))
            {
                return;
            }

            bool deleted = board.DeleteGear(node);
            if (!deleted)
            {
                board.SnapBackToOriginal(node);
            }
        }

        private void LogBoardDropFailure(Exception ex, IGridNode node)
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

        private void TryTrashInventoryGear(GearConfigData gear)
        {
            if (gear == null || (engineService != null && engineService.IsRunning))
            {
                return;
            }

            presentationTransfer.TrashInventoryGear(gear);
            Debug.Log($"<color=#ff5555>[TrashZoneViewModel]</color> Inventory Item '{gear.Id}' Trashed! Reward: {gear.DeleteRewardAmount}");
        }
    }
}
