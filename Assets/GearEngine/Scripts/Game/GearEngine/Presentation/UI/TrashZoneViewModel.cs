using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class TrashZoneViewModel : ViewModel
    {
        public TrashZoneViewModel(IGearEngineService engineService, BoardViewModel board, IGearPresentationTransferService presentationTransfer, GearEngineFeatureToggleSO featureToggle)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.presentationTransfer = presentationTransfer ?? throw new ArgumentNullException(nameof(presentationTransfer));
            this.featureToggle = featureToggle;
        }

        internal GearEngineFeatureToggleSO FeatureToggleForTrashPlacement => featureToggle;

        private readonly IGearEngineService engineService;
        private readonly BoardViewModel board;
        private readonly IGearPresentationTransferService presentationTransfer;
        private readonly GearEngineFeatureToggleSO featureToggle;

        [ObservableProperty]
        private bool isActive;

        [ObservableProperty]
        private string rewardText = string.Empty;

        public void HandleDragStarted(DragPayload payload)
        {
            GearConfigData gear = payload.GetData<GearConfigData>() ?? payload.GetData<IGridNode>()?.ConfigData;
            if (gear != null && CanTrashAcceptGear(gear))
            {
                RewardText = $"+{gear.DeleteRewardAmount}";
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

        public bool HandleBoardGearDropped(IGridNode node)
        {
            try
            {
                return TryDeleteBoardGear(node);
            }
            catch (Exception ex)
            {
                LogBoardDropFailure(ex, node);
                return false;
            }
        }

        public bool HandleInventoryGearDropped(GearConfigData gear)
        {
            try
            {
                return TryTrashInventoryGear(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrashZoneViewModel] HandleInventoryGearDropped failed: {ex.Message}\n{ex.StackTrace}");
                return false;
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

        private bool TryDeleteBoardGear(IGridNode node)
        {
            if (node == null || (engineService != null && engineService.IsRunning))
            {
                return false;
            }

            return board.DeleteGear(node);
        }

        private void LogBoardDropFailure(Exception ex, IGridNode node)
        {
            Debug.LogError($"[TrashZoneViewModel] HandleBoardGearDropped failed: {ex.Message}\n{ex.StackTrace}");
        }

        private bool TryTrashInventoryGear(GearConfigData gear)
        {
            if (gear == null || (engineService != null && engineService.IsRunning))
            {
                return false;
            }

            presentationTransfer.TrashInventoryGear(gear);
            Debug.Log($"<color=#ff5555>[TrashZoneViewModel]</color> Inventory Item '{gear.Id}' Trashed! Reward: {gear.DeleteRewardAmount}");
            return true;
        }
    }
}
