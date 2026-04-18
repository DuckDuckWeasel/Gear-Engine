using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearTrashFeature : IDisposable
    {
        public GearTrashFeature(BoardViewModel boardVm, GearInventoryViewModel inventoryVm, Canvas canvas, BoardConfigSO boardConfig, IDragService dragService, TrashZoneAlignment alignment = TrashZoneAlignment.Right, float yOffset = 80f, TagSO trashZoneTag = null, Sprite trashIcon = null)
        {
            this.boardVm = boardVm ?? throw new ArgumentNullException(nameof(boardVm));
            this.inventoryVm = inventoryVm;
            _ = canvas ?? throw new ArgumentNullException(nameof(canvas));
            this.dragService = dragService;

            Vector3 gridAnchorPoint = BuildGridAnchor(boardConfig, alignment);
            Vector2 pivot = BuildPivot(alignment);
            trashZone = TrashDropZoneFactory.Create(canvas, gridAnchorPoint, new Vector2(0f, yOffset), pivot, trashZoneTag, trashIcon);
            SubscribeTrashZone();
            SubscribeDragService();
        }

        private readonly BoardViewModel boardVm;
        private readonly GearInventoryViewModel inventoryVm;
        private readonly TrashDropZoneView trashZone;
        private readonly IDragService dragService;

        private IGridNode pendingTrashNode;
        private GearConfigData pendingTrashConfigData;
        private bool isDisposed;

        private void HandleDragServiceStarted(object data)
        {
            if (isDisposed)
            {
                return;
            }

            GearConfigData gearData = data as GearConfigData;
            Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Drag started - gear: {gearData?.Id ?? "null"}, deletable: {gearData?.IsDeletable ?? false}");
            trashZone?.OnDragStarted(gearData);
        }

        private void HandleDragServiceEnded()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Drag ended - hiding trash zone.");
            trashZone?.OnDragEnded();
        }

        private void HandleInventoryGearDropped(GearConfigData data)
        {
            if (isDisposed || data == null || !data.IsDeletable)
            {
                return;
            }

            Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> Inventory gear '{data.Id}' dropped directly on trash. Removed and destroyed. Reward: +{data.DeleteRewardAmount}.");
            ConsumeInventoryGear(data);
            trashZone.OnDragEnded();
        }

        public void OnTrashDropRequested(IGridNode node)
        {
            if (isDisposed)
            {
                return;
            }

            if (ShouldRejectTrashDrop(node))
            {
                return;
            }

            pendingTrashNode = node;
            pendingTrashConfigData = node.ConfigData;
            Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Trash drop requested for '{node.ConfigData.Id}' at {node.Position}. Reward: +{node.ConfigData.DeleteRewardAmount}.");
            ExecuteDeletion();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            pendingTrashNode = null;
            UnsubscribeDragService();
            if (trashZone != null)
            {
                trashZone.OnInventoryGearDropped -= HandleInventoryGearDropped;
            }

            if (trashZone != null)
            {
                UnityEngine.Object.Destroy(trashZone.gameObject);
            }
        }

        private void ExecuteDeletion()
        {
            try
            {
                TryDeletePendingNode();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTrashFeature] ExecuteDeletion failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                CleanupAfterDeletion();
            }
        }

        private void SubscribeTrashZone()
        {
            trashZone.OnInventoryGearDropped += HandleInventoryGearDropped;
        }

        private void SubscribeDragService()
        {
            if (dragService == null)
            {
                return;
            }

            dragService.OnDragStarted += HandleDragServiceStarted;
            dragService.OnDragEnded += HandleDragServiceEnded;
        }

        private void UnsubscribeDragService()
        {
            if (dragService == null)
            {
                return;
            }

            dragService.OnDragStarted -= HandleDragServiceStarted;
            dragService.OnDragEnded -= HandleDragServiceEnded;
        }

        private void ConsumeInventoryGear(GearConfigData data)
        {
            if (inventoryVm == null)
            {
                return;
            }

            inventoryVm.ConsumeSpecificGear(data);
            boardVm.GrantTrashReward(data.DeleteRewardAmount);
        }

        private bool ShouldRejectTrashDrop(IGridNode node)
        {
            if (node != null && node.ConfigData != null && node.ConfigData.IsDeletable)
            {
                return false;
            }

            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Trash drop rejected - gear is not deletable. Snapping back.");
            boardVm.SnapBackToOriginal(node);
            return true;
        }

        private Vector2 BuildPivot(TrashZoneAlignment alignment)
        {
            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return new Vector2(0f, 0.5f);
                case TrashZoneAlignment.Center:
                    return new Vector2(0.5f, 0.5f);
                default:
                    return new Vector2(1f, 0.5f);
            }
        }

        private Vector3 BuildGridAnchor(BoardConfigSO boardConfig, TrashZoneAlignment alignment)
        {
            if (boardConfig == null)
            {
                return Vector3.zero;
            }

            GetTopRowAnchors(boardConfig, out Vector3 topLeft, out Vector3 topRight);
            return ResolveAlignedAnchor(alignment, topLeft, topRight);
        }

        private void GetTopRowAnchors(BoardConfigSO boardConfig, out Vector3 topLeft, out Vector3 topRight)
        {
            int topY = boardConfig.GridHeight - 1;
            topLeft = boardConfig.GetWorldPosition(new Vector2Int(0, topY));
            topRight = boardConfig.GetWorldPosition(new Vector2Int(boardConfig.GridWidth - 1, topY));
        }

        private Vector3 ResolveAlignedAnchor(TrashZoneAlignment alignment, Vector3 topLeft, Vector3 topRight)
        {
            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return topLeft;
                case TrashZoneAlignment.Center:
                    return (topLeft + topRight) * 0.5f;
                default:
                    return topRight;
            }
        }

        private void TryDeletePendingNode()
        {
            if (pendingTrashNode == null)
            {
                return;
            }

            bool deleted = boardVm.DeleteGear(pendingTrashNode);
            LogDeletionOutcome(deleted);
        }

        private void LogDeletionOutcome(bool deleted)
        {
            string gearId = pendingTrashConfigData?.Id ?? "Unknown";
            Vector2Int pos = pendingTrashNode.Position;
            int reward = pendingTrashConfigData?.DeleteRewardAmount ?? 0;
            if (deleted)
            {
                Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> DELETE EXECUTED - '{gearId}' at {pos} destroyed. Reward: +{reward}. Gear removed from board.");
                return;
            }

            Debug.LogError($"[GearTrashFeature] DELETE FAILED - BoardViewModel.DeleteGear returned false for '{gearId}' at {pos}.");
        }

        private void CleanupAfterDeletion()
        {
            pendingTrashNode = null;
            pendingTrashConfigData = null;
            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Hiding trash zone after execution.");
            trashZone?.OnDragEnded();
        }
    }
}
