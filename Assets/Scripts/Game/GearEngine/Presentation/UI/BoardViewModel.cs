using System;
using System.Collections.Generic;
using Game.GearEngine;
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

        private Vector2Int pickupOriginalPos;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;

        public IGearEngineService EngineService => engineService;

        public BoardConfigSO BoardConfig => boardConfig;

        public void Initialize(
            IGearEngineService engineService,
            IGridManager gridManager,
            GearNodeFactory nodeFactory,
            BoardConfigSO boardConfig)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
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
        }

        public void OnGearDropped(IGridNode node, Vector2Int toPos)
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
