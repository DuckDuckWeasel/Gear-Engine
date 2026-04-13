using System;
using System.Collections.Generic;
using Game.GearEngine;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public sealed class BoardViewModel : ViewModel
    {
        private IGearEngineService engineService;
        private IGridManager gridManager;
        private GearNodeFactory nodeFactory;
        private BoardConfigSO boardConfig;
        private IEventBus eventBus;
        private GearEngineFeatureToggleSO featureToggle;

        private Vector2Int pickupOriginalPos;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;
        public event Action<GearConfigData> OnBoardDragStarted;
        public event Action OnBoardDragEnded;

        public IGearEngineService EngineService => engineService;

        public BoardConfigSO BoardConfig => boardConfig;

        public void Initialize(
            IGearEngineService engineService,
            IGridManager gridManager,
            GearNodeFactory nodeFactory,
            BoardConfigSO boardConfig,
            IEventBus eventBus = null,
            GearEngineFeatureToggleSO featureToggle = null)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.eventBus = eventBus;
            this.featureToggle = featureToggle;
        }

        public IGridNode GetNode(Vector2Int coord) => gridManager.GetNode(coord);

        public IEnumerable<IGridNode> GetCurrentNodes() => gridManager.GetAllNodes();

        public void LoadLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (gridManager == null || nodeFactory == null || boardConfig == null)
            {
                throw new InvalidOperationException("BoardViewModel must be initialized before LoadLayout.");
            }

            foreach (BoardGearPlacementData placement in layout.Placements)
            {
                if (placement == null || placement.GearConfig == null)
                {
                    continue;
                }

                Vector2Int pos = placement.Position;
                bool inBounds =
                    pos.x >= 0 && pos.x < boardConfig.GridWidth &&
                    pos.y >= 0 && pos.y < boardConfig.GridHeight;

                if (!inBounds)
                {
                    Debug.LogError($"[BoardViewModel] Ignoring out-of-bounds starting gear at {pos}.");
                    continue;
                }

                if (gridManager.GetNode(pos) != null)
                {
                    Debug.LogError($"[BoardViewModel] Duplicate starting gear at {pos}.");
                    continue;
                }

                GearConfigData runtimeData = placement.GearConfig.CreateRuntimeData();
                IGridNode node = nodeFactory.CreateNode(pos, runtimeData);
                gridManager.AddNode(node);
            }
        }

        public void OnGearPickedUp(IGridNode node, Vector2Int fromPos)
        {
            if (node == null || gridManager == null)
            {
                return;
            }

            pickupOriginalPos = fromPos;
            gridManager.ExtractNode(fromPos);
            OnBoardDragStarted?.Invoke(node.ConfigData);
        }

        public void OnGearDropped(IGridNode node, Vector2Int toPos)
        {
            try
            {
                OnGearDroppedInternal(node, toPos);
            }
            finally
            {
                OnBoardDragEnded?.Invoke();
            }
        }

        private void OnGearDroppedInternal(IGridNode node, Vector2Int toPos)
        {
            if (node == null || engineService == null || gridManager == null || boardConfig == null)
            {
                return;
            }

            if (engineService.IsRunning)
            {
                return;
            }

            bool isValidDrop = toPos.x >= 0 && toPos.x < boardConfig.GridWidth &&
                               toPos.y >= 0 && toPos.y < boardConfig.GridHeight;

            if (!isValidDrop)
            {
                SnapNodeBackToOriginal(node);
                Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> Drop at {toPos} out of bounds. Snapped back.");
                return;
            }

            IGridNode occupant = gridManager.GetNode(toPos);

            if (occupant == null)
            {
                PlaceNodeAt(node, toPos);
                Debug.Log($"<color=#55ff55>[BoardViewModel]</color> Successfully dropped gear into {toPos}");
                return;
            }

            GearConfigData draggedData = node.ConfigData;
            GearConfigData occupantData = occupant.ConfigData;

            // Reject drop if either the occupant or the dragged gear cannot be moved
            if ((occupantData != null && !occupantData.IsMovable) || 
                (draggedData != null && !draggedData.IsMovable))
            {
                SnapNodeBackToOriginal(node);
                Debug.Log($"<color=#ff5555>[BoardViewModel]</color> Cannot swap '{draggedData?.Id}' with '{occupantData?.Id}' at {toPos} — at least one gear is not movable. Snapped back.");
                return;
            }

            if (occupantData.Id == draggedData.Id && occupantData.NextLevelConfig != null)
            {
                MergeBoardGearsAt(node, occupant, toPos, occupantData);
                return;
            }

            SwapBoardGears(node, occupant, toPos);
            Debug.Log($"<color=#ffff33>[BoardViewModel]</color> Swapped positions! {toPos} <-> {pickupOriginalPos}");
        }

        /// <summary>
        /// Disposes the logical node after a board gear is dragged over UI (view is destroyed separately).
        /// </summary>
        public void HandleBoardGearReturnedOverUI(IGridNode node)
        {
            try
            {
                node?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] HandleBoardGearReturnedOverUI failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                OnBoardDragEnded?.Invoke();
            }
        }

        /// <summary>
        /// Permanently removes a gear from the board and returns the reward amount.
        /// Called after the player confirms deletion in the confirmation popup.
        /// </summary>
        /// <returns>True if the gear was successfully deleted.</returns>
        public bool DeleteGear(IGridNode node)
        {
            try
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (node.ConfigData == null || !node.ConfigData.IsDeletable)
                {
                    Debug.LogWarning($"[BoardViewModel] DeleteGear rejected: gear is not deletable.");
                    return false;
                }

                if (featureToggle != null && !featureToggle.EnableTrashDeletion)
                {
                    Debug.LogWarning($"[BoardViewModel] DeleteGear rejected: trash deletion feature is disabled.");
                    return false;
                }

                if (engineService != null && engineService.IsRunning)
                {
                    Debug.LogWarning($"[BoardViewModel] DeleteGear rejected: simulation is running.");
                    return false;
                }

                Vector2Int pos = node.Position;
                int reward = node.ConfigData.DeleteRewardAmount;

                IGridNode extracted = gridManager.ExtractNode(pos);

                // If it wasn't in the grid (because it's actively being dragged), 
                // we fall back to the dragged node itself.
                if (extracted == null)
                {
                    extracted = node;
                }
                else if (extracted != node)
                {
                    // Safey check: if another node somehow occupied this pos, put it back
                    gridManager.AddNode(extracted);
                    extracted = node;
                }

                OnGearRemoved?.Invoke(extracted);
                extracted.Dispose();

                eventBus?.Raise(new GearDeletedEvent(pos, reward));
                Debug.Log($"<color=#ff5555>[BoardViewModel]</color> Gear at {pos} DELETED. Reward: {reward}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] DeleteGear failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Grants a scrap reward for deleting a gear directly from the inventory.
        /// </summary>
        public void GrantTrashReward(int rewardAmount)
        {
            if (rewardAmount > 0)
            {
                eventBus?.Raise(new GearDeletedEvent(Vector2Int.zero, rewardAmount));
            }
        }

        /// <summary>
        /// Snaps a gear back to its pickup position. Used when the player cancels deletion.
        /// </summary>
        public void SnapBackToOriginal(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            SnapNodeBackToOriginal(node);
            OnBoardDragEnded?.Invoke();
        }

        /// <summary>
        /// Places or merges inventory gear onto the board. Does not modify inventory; the screen consumes on success.
        /// </summary>
        /// <returns>True if a node was placed or merged.</returns>
        public bool HandleInventoryDrop(Vector3 worldPosition, GearConfigData gearData)
        {
            try
            {
                if (gearData == null)
                {
                    throw new ArgumentNullException(nameof(gearData));
                }

                if (gridManager == null || boardConfig == null || engineService == null || engineService.IsRunning)
                {
                    return false;
                }

                Vector2Int targetDropPos = boardConfig.GetGridPosition(worldPosition);
                IGridNode occupant = gridManager.GetNode(targetDropPos);

                if (occupant == null)
                {
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, gearData);
                    gridManager.AddNode(newNode);
                    OnGearPlaced?.Invoke(newNode);
                    return true;
                }

                GearConfigData occupantData = occupant.ConfigData;

                if (occupantData.Id == gearData.Id && occupantData.NextLevelConfig != null)
                {
                    IGridNode removedOccupant = gridManager.ExtractNode(targetDropPos);
                    if (removedOccupant != occupant)
                    {
                        Debug.LogError("[BoardViewModel] Grid state mismatch during UI merge.");
                        return false;
                    }

                    OnGearRemoved?.Invoke(occupant);
                    occupant.Dispose();

                    GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
                    gridManager.AddNode(newNode);
                    OnGearPlaced?.Invoke(newNode);
                    Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED UI {gearData.Id} into {upgradedData.Id} at {targetDropPos}!");
                    return true;
                }

                Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> UI Drop Cancelled! {gearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] HandleInventoryDrop failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private void PlaceNodeAt(IGridNode node, Vector2Int toPos)
        {
            ((NodeBase)node).Position = toPos;
            gridManager.AddNode(node);
        }

        private void SnapNodeBackToOriginal(IGridNode node)
        {
            ((NodeBase)node).Position = pickupOriginalPos;
            gridManager.AddNode(node);
        }

        private void SwapBoardGears(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos)
        {
            gridManager.ExtractNode(targetDropPos);

            ((NodeBase)draggedNode).Position = targetDropPos;
            gridManager.AddNode(draggedNode);

            ((NodeBase)occupantNode).Position = pickupOriginalPos;
            gridManager.AddNode(occupantNode);
        }

        private void MergeBoardGearsAt(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos, GearConfigData occupantData)
        {
            gridManager.ExtractNode(targetDropPos);

            OnGearRemoved?.Invoke(draggedNode);
            OnGearRemoved?.Invoke(occupantNode);

            draggedNode.Dispose();
            occupantNode.Dispose();

            GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
            IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
            gridManager.AddNode(newNode);
            OnGearPlaced?.Invoke(newNode);
            Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED board gears into {upgradedData.Id} at {targetDropPos}!");
        }
    }
}
