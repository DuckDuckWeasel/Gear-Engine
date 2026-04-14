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
        public GearTrashFeature(BoardViewModel boardVm, GearInventoryViewModel inventoryVm, Canvas canvas, BoardConfigSO boardConfig, TrashZoneAlignment alignment = TrashZoneAlignment.Right, TagSO trashZoneTag = null, Sprite trashIcon = null)
        {
            this.boardVm = boardVm ?? throw new ArgumentNullException(nameof(boardVm));
            this.inventoryVm = inventoryVm;
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));

            Vector3 gridAnchorPoint = CreateGridAnchor(boardConfig, alignment);
            Vector2 pivot = CreatePivot(alignment);
            trashZone = TrashDropZoneFactory.Create(canvas, gridAnchorPoint, new Vector2(0f, trashZoneYOffset), pivot, trashZoneTag, trashIcon);
            trashZone.OnInventoryGearDropped += HandleInventoryGearDropped;
        }

        private static readonly float trashZoneYOffset = 160f;

        private readonly BoardViewModel boardVm;
        private readonly GearInventoryViewModel inventoryVm;
        private readonly Canvas canvas;
        private readonly TrashDropZoneView trashZone;

        private IGridNode pendingTrashNode;
        private GearConfigData pendingTrashConfigData;
        private bool isDisposed;

        public void OnDragStarted(GearConfigData data)
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Drag started — gear: {data?.Id ?? "null"}, deletable: {data?.IsDeletable ?? false}");
            trashZone?.OnDragStarted(data);
        }

        public void OnDragEnded()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Drag ended — hiding trash zone.");
            trashZone?.OnDragEnded();
        }

        public void OnTrashDropRequested(IGridNode node)
        {
            if (isDisposed)
            {
                return;
            }

            if (node == null || node.ConfigData == null || !node.ConfigData.IsDeletable)
            {
                Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Trash drop rejected — gear is not deletable. Snapping back.");
                boardVm.SnapBackToOriginal(node);
                return;
            }

            Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Trash drop requested for '{node.ConfigData.Id}' at {node.Position}. Reward: +{node.ConfigData.DeleteRewardAmount}.");
            pendingTrashNode = node;
            pendingTrashConfigData = node.ConfigData;
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

            if (trashZone != null)
            {
                UnityEngine.Object.Destroy(trashZone.gameObject);
            }
        }

        private void HandleInventoryGearDropped(GearConfigData data)
        {
            if (isDisposed || data == null || !data.IsDeletable)
            {
                return;
            }

            Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> Inventory gear '{data.Id}' dropped directly on trash. Removed and destroyed. Reward: +{data.DeleteRewardAmount}.");

            if (inventoryVm != null)
            {
                inventoryVm.ConsumeSpecificGear(data);
                boardVm.GrantTrashReward(data.DeleteRewardAmount);
            }

            trashZone.OnDragEnded();
        }

        private void ExecuteDeletion()
        {
            try
            {
                TryDeletePendingGear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTrashFeature] ExecuteDeletion failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                FinishDeletionCleanup();
            }
        }

        private void TryDeletePendingGear()
        {
            if (pendingTrashNode == null)
            {
                return;
            }

            bool deleted = boardVm.DeleteGear(pendingTrashNode);
            LogTrashDeletionOutcome(deleted);
        }

        private void LogTrashDeletionOutcome(bool deleted)
        {
            string gearId = pendingTrashConfigData?.Id ?? "Unknown";
            Vector2Int pos = pendingTrashNode.Position;
            int reward = pendingTrashConfigData?.DeleteRewardAmount ?? 0;

            if (deleted)
            {
                Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> DELETE EXECUTED — '{gearId}' at {pos} destroyed. Reward: +{reward}. Gear removed from board.");
            }
            else
            {
                Debug.LogError($"[GearTrashFeature] DELETE FAILED — BoardViewModel.DeleteGear returned false for '{gearId}' at {pos}.");
            }
        }

        private void FinishDeletionCleanup()
        {
            pendingTrashNode = null;
            pendingTrashConfigData = null;
            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Hiding trash zone after execution.");
            trashZone?.OnDragEnded();
        }

        private static Vector2 CreatePivot(TrashZoneAlignment alignment)
        {
            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return new Vector2(0f, 0.5f);
                case TrashZoneAlignment.Center:
                    return new Vector2(0.5f, 0.5f);
                case TrashZoneAlignment.Right:
                default:
                    return new Vector2(1f, 0.5f);
            }
        }

        private static Vector3 CreateGridAnchor(BoardConfigSO boardConfig, TrashZoneAlignment alignment)
        {
            if (boardConfig == null)
            {
                return Vector3.zero;
            }

            (Vector3 topLeft, Vector3 topRight) = BuildTopRowWorldCorners(boardConfig);

            switch (alignment)
            {
                case TrashZoneAlignment.Left:
                    return topLeft;
                case TrashZoneAlignment.Center:
                    return (topLeft + topRight) * 0.5f;
                case TrashZoneAlignment.Right:
                default:
                    return topRight;
            }
        }

        private static (Vector3 topLeft, Vector3 topRight) BuildTopRowWorldCorners(BoardConfigSO boardConfig)
        {
            int topY = boardConfig.GridHeight - 1;
            Vector3 topLeft = boardConfig.GetWorldPosition(new Vector2Int(0, topY));
            Vector3 topRight = boardConfig.GetWorldPosition(new Vector2Int(boardConfig.GridWidth - 1, topY));
            return (topLeft, topRight);
        }
    }
}
