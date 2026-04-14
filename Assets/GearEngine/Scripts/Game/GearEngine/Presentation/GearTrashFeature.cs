using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    /// <summary>
    /// Mediator that encapsulates all trash/scrap deletion coordination.
    /// Owns the trash zone view, pending node state, popup spawning, and confirm/cancel callbacks.
    /// Created by <see cref="GearEngineView"/> only when the feature toggle is enabled.
    /// Subscribes to <see cref="IDragService"/> for drag lifecycle events.
    /// </summary>
    public sealed class GearTrashFeature : IDisposable
    {
        private readonly BoardViewModel boardVm;
        private readonly GearInventoryViewModel inventoryVm;
        private readonly Canvas canvas;
        private readonly TrashDropZoneView trashZone;
        private readonly IDragService dragService;

        private IGridNode pendingTrashNode;
        private GearConfigData pendingTrashConfigData;
        private bool isDisposed;

        /// <param name="boardVm">Board view model for deletion and snap-back.</param>
        /// <param name="inventoryVm">Inventory view model for removing trashed gears from inventory.</param>
        /// <param name="canvas">Canvas to parent the trash zone and popups.</param>
        /// <param name="boardConfig">Board config used to compute the grid's top edge.</param>
        /// <param name="dragService">Centralized drag service for drag lifecycle events.</param>
        /// <param name="alignment">Horizontal alignment relative to the grid (Left, Center, Right).</param>
        /// <param name="yOffset">Vertical pixel offset above the grid's top edge.</param>
        /// <param name="trashZoneTag">Tag for discovery by the drag handler via the tag system.</param>
        /// <param name="trashIcon">Sprite for the trash icon.</param>
        public GearTrashFeature(
            BoardViewModel boardVm,
            GearInventoryViewModel inventoryVm,
            Canvas canvas,
            BoardConfigSO boardConfig,
            IDragService dragService,
            TrashZoneAlignment alignment = TrashZoneAlignment.Right,
            float yOffset = 80f,
            TagSO trashZoneTag = null,
            Sprite trashIcon = null)
        {
            this.boardVm = boardVm ?? throw new ArgumentNullException(nameof(boardVm));
            this.inventoryVm = inventoryVm;
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            this.dragService = dragService;

            Vector3 gridAnchorPoint = ComputeGridAnchor(boardConfig, alignment);
            Vector2 pivot = ComputePivot(alignment);

            trashZone = TrashDropZoneFactory.Create(
                canvas, gridAnchorPoint, new Vector2(0f, yOffset), pivot, trashZoneTag, trashIcon);

            trashZone.OnInventoryGearDropped += HandleInventoryGearDropped;

            // Subscribe to centralized drag service
            if (dragService != null)
            {
                dragService.OnDragStarted += HandleDragServiceStarted;
                dragService.OnDragEnded += HandleDragServiceEnded;
            }
        }

        private void HandleDragServiceStarted(object data)
        {
            if (isDisposed)
            {
                return;
            }

            GearConfigData gearData = data as GearConfigData;
            Debug.Log($"<color=#ff9900>[GearTrashFeature]</color> Drag started — gear: {gearData?.Id ?? "null"}, deletable: {gearData?.IsDeletable ?? false}");
            trashZone?.OnDragStarted(gearData);
        }

        private void HandleDragServiceEnded()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Drag ended — hiding trash zone.");
            trashZone?.OnDragEnded();
        }

        private void HandleInventoryGearDropped(GearConfigData data)
        {
            if (isDisposed || data == null || !data.IsDeletable) return;

            Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> Inventory gear '{data.Id}' dropped directly on trash. Removed and destroyed. Reward: +{data.DeleteRewardAmount}.");

            if (inventoryVm != null)
            {
                inventoryVm.ConsumeSpecificGear(data);
                
                // Fire reward event using BoardViewModel's event integration wrapper logic since there's no central one
                boardVm.GrantTrashReward(data.DeleteRewardAmount);
            }
            
            trashZone.OnDragEnded();
        }

        /// <summary>
        /// Returns a pivot that aligns the correct edge of the trash zone with the grid corner.
        /// Left alignment → right edge at corner (pivot 1, 0.5).
        /// Right alignment → left edge at corner (pivot 0, 0.5).
        /// Center → centered on anchor (pivot 0.5, 0.5).
        /// </summary>
        private static Vector2 ComputePivot(TrashZoneAlignment alignment)
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

        private static Vector3 ComputeGridAnchor(BoardConfigSO boardConfig, TrashZoneAlignment alignment)
        {
            if (boardConfig == null)
            {
                return Vector3.zero;
            }

            int topY = boardConfig.GridHeight - 1;
            Vector3 topLeft = boardConfig.GetWorldPosition(new Vector2Int(0, topY));
            Vector3 topRight = boardConfig.GetWorldPosition(new Vector2Int(boardConfig.GridWidth - 1, topY));

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

        /// <summary>
        /// Called when a gear is dropped on the trash zone.
        /// Validates the gear and performs immediate deletion.
        /// </summary>
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

        private void ExecuteDeletion()
        {
            try
            {
                if (pendingTrashNode != null)
                {
                    string gearId = pendingTrashConfigData?.Id ?? "Unknown";
                    Vector2Int pos = pendingTrashNode.Position;
                    int reward = pendingTrashConfigData?.DeleteRewardAmount ?? 0;

                    bool deleted = boardVm.DeleteGear(pendingTrashNode);

                    if (deleted)
                    {
                        Debug.Log($"<color=#ff5555>[GearTrashFeature]</color> DELETE EXECUTED — '{gearId}' at {pos} destroyed. Reward: +{reward}. Gear removed from board.");
                    }
                    else
                    {
                        Debug.LogError($"[GearTrashFeature] DELETE FAILED — BoardViewModel.DeleteGear returned false for '{gearId}' at {pos}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTrashFeature] ExecuteDeletion failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                pendingTrashNode = null;
                pendingTrashConfigData = null;
                Debug.Log("<color=#ff9900>[GearTrashFeature]</color> Hiding trash zone after execution.");
                trashZone?.OnDragEnded();
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            pendingTrashNode = null;

            if (dragService != null)
            {
                dragService.OnDragStarted -= HandleDragServiceStarted;
                dragService.OnDragEnded -= HandleDragServiceEnded;
            }

            if (trashZone != null)
            {
                UnityEngine.Object.Destroy(trashZone.gameObject);
            }
        }
    }
}
