using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Merge;
using GearEngine.GearEngine.Nodes;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Services.Board
{
    public sealed class BoardService : IBoardService
    {
        public BoardService(
            IGridManager gridManager,
            IGearNodeFactory nodeFactory,
            BoardRulesSO boardRules,
            IGearEngineService engineService,
            IGridSwapService swapService,
            IGridMergeService mergeService,
            GearEngineFeatureToggleSO featureToggle,
            IEventBus eventBus,
            GearBoardLoadoutData boardLoadout)
        {
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardRules = boardRules ?? throw new ArgumentNullException(nameof(boardRules));
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.swapService = swapService;
            this.mergeService = mergeService;
            this.featureToggle = featureToggle;
            this.eventBus = eventBus;

            boardModel = new BoardModel
            {
                BoardRules = boardRules
            };

            boardLoadout ??= new GearBoardLoadoutData();
            if (boardLoadout.BoardLayout != null)
            {
                LoadLayout(boardLoadout.BoardLayout);
            }

            SyncBoardModel();
        }

        public event Action<IGridNode> GearPlaced;
        public event Action<IGridNode> GearRemoved;

        public BoardModel GetBoard() => boardModel;

        public BoardRulesSO BoardRules => boardRules;

        public bool IsSimulationRunning => gridManager != null && gridManager.IsRunning;

        public int CurrentBoardGearCount => gridManager?.GetAllNodes().Count() ?? 0;

        public int MaxAllowedBoardGears => boardRules != null ? boardRules.MaxAllowedBoardGears : int.MaxValue;

        private readonly IGridManager gridManager;
        private readonly IGearNodeFactory nodeFactory;
        private readonly BoardRulesSO boardRules;
        private readonly IGearEngineService engineService;
        private readonly IGridSwapService swapService;
        private readonly IGridMergeService mergeService;
        private readonly GearEngineFeatureToggleSO featureToggle;
        private readonly IEventBus eventBus;
        private readonly BoardModel boardModel;

        public IGridNode GetNode(Vector2Int coord) => gridManager.GetNode(coord);

        public IEnumerable<IGridNode> GetAllNodes() => gridManager.GetAllNodes();

        public void ToggleSimulation()
        {
            try
            {
                if (gridManager == null)
                {
                    throw new InvalidOperationException("Grid manager is not available.");
                }

                if (gridManager.IsRunning)
                {
                    gridManager.Stop();
                }
                else
                {
                    gridManager.Play();
                }

                SyncBoardModel();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardService] ToggleSimulation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void LoadLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (gridManager == null || nodeFactory == null || boardRules == null)
            {
                throw new InvalidOperationException("[BoardService] LoadLayout called before dependencies are ready.");
            }

            if (engineService != null && engineService.IsRunning)
            {
                engineService.Stop();
            }

            foreach (IGridNode node in gridManager.GetAllNodes().ToArray())
            {
                GearRemoved?.Invoke(node);
            }

            gridManager.ClearAll();

            foreach (BoardGearPlacementData placement in layout.Placements)
            {
                if (placement == null || placement.GearConfig == null)
                {
                    continue;
                }

                Vector2Int pos = placement.Position;
                bool inBounds = pos.x >= 0 && pos.x < boardRules.GridWidth && pos.y >= 0 && pos.y < boardRules.GridHeight;

                if (!inBounds)
                {
                    Debug.LogError($"[BoardService] Ignoring out-of-bounds starting gear at {pos}.");
                    continue;
                }

                if (gridManager.GetNode(pos) != null)
                {
                    Debug.LogError($"[BoardService] Duplicate starting gear at {pos}.");
                    continue;
                }

                GearConfigData runtimeData = placement.GearConfig.CreateRuntimeData();
                IGridNode node = nodeFactory.CreateNode(pos, runtimeData);
                gridManager.AddNode(node);
            }

            SyncBoardModel();
        }

        public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos)
        {
            if (node == null || engineService == null || gridManager == null || boardRules == null)
            {
                return false;
            }

            if (engineService.IsRunning)
            {
                return false;
            }

            bool isValidDrop = toPos.x >= 0 && toPos.x < boardRules.GridWidth &&
                               toPos.y >= 0 && toPos.y < boardRules.GridHeight;

            if (!isValidDrop)
            {
                SnapNodeBackToOriginal(node, fromPos);
                Debug.LogWarning($"<color=#ff5555>[BoardService]</color> Drop at {toPos} out of bounds. Snapped back.");
                return false;
            }

            IGridNode occupant = gridManager.GetNode(toPos);

            if (occupant == null)
            {
                if (gridManager.GetNode(fromPos) == node)
                {
                    gridManager.ExtractNode(fromPos);
                }

                PlaceNodeAt(node, toPos);
                Debug.Log($"<color=#55ff55>[BoardService]</color> Successfully dropped gear into {toPos}");
                SyncBoardModel();
                return true;
            }

            GearConfigData draggedData = node.ConfigData;
            GearConfigData occupantData = occupant.ConfigData;

            if ((occupantData != null && !occupantData.IsMovable) ||
                (draggedData != null && !draggedData.IsMovable))
            {
                SnapNodeBackToOriginal(node, fromPos);
                Debug.Log($"<color=#ff5555>[BoardService]</color> Cannot swap '{draggedData?.Id}' with '{occupantData?.Id}' at {toPos} — at least one gear is not movable. Snapped back.");
                return false;
            }

            if (occupantData.Id == draggedData.Id && occupantData.NextLevelConfig != null)
            {
                MergeBoardGearsAt(node, occupant, toPos, occupantData, fromPos);
                SyncBoardModel();
                return true;
            }

            SwapBoardGears(node, occupant);
            Debug.Log($"<color=#ffff33>[BoardService]</color> Swapped positions! {toPos} <-> {fromPos}");
            SyncBoardModel();
            return true;
        }

        public bool TryPlace(Vector2Int targetDropPos, GearConfigData gearData)
        {
            try
            {
                if (gearData == null)
                {
                    throw new ArgumentNullException(nameof(gearData));
                }

                if (gridManager == null || boardRules == null || engineService == null || engineService.IsRunning)
                {
                    return false;
                }

                IGridNode occupant = gridManager.GetNode(targetDropPos);

                if (occupant == null)
                {
                    if (CurrentBoardGearCount >= boardRules.MaxAllowedBoardGears)
                    {
                        Debug.LogWarning($"<color=#ff5555>[BoardService]</color> Board limit reached ({CurrentBoardGearCount}/{boardRules.MaxAllowedBoardGears}). Cannot place gear.");
                        return false;
                    }

                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, gearData);
                    gridManager.AddNode(newNode);
                    PlaceGear(newNode);
                    SyncBoardModel();
                    return true;
                }

                GearConfigData occupantData = occupant.ConfigData;

                if (occupantData.Id == gearData.Id && occupantData.NextLevelConfig != null)
                {
                    IGridNode removedOccupant = gridManager.ExtractNode(targetDropPos);
                    if (removedOccupant != occupant)
                    {
                        Debug.LogError("[BoardService] Grid state mismatch during UI merge.");
                        return false;
                    }

                    RemoveGear(occupant);

                    GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
                    gridManager.AddNode(newNode);
                    PlaceGear(newNode);
                    Debug.Log($"<color=#ffaa55>[BoardService]</color> MERGED UI {gearData.Id} into {upgradedData.Id} at {targetDropPos}!");
                    SyncBoardModel();
                    return true;
                }

                Debug.LogWarning($"<color=#ff5555>[BoardService]</color> UI Drop Cancelled! {gearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardService] TryPlace failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public bool TryRemoveBoardGear(IGridNode node)
        {
            if (node == null)
            {
                return false;
            }

            try
            {
                GearRemoved?.Invoke(node);
                node.Dispose();
                SyncBoardModel();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardService] TryRemoveBoardGear failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public bool TryDeleteBoardGear(IGridNode node)
        {
            try
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                if (node.ConfigData == null || !node.ConfigData.IsDeletable)
                {
                    Debug.LogWarning("[BoardService] TryDeleteBoardGear rejected: gear is not deletable.");
                    return false;
                }

                if (featureToggle != null && !featureToggle.EnableTrashDeletion)
                {
                    Debug.LogWarning("[BoardService] TryDeleteBoardGear rejected: trash deletion feature is disabled.");
                    return false;
                }

                if (engineService != null && engineService.IsRunning)
                {
                    Debug.LogWarning("[BoardService] TryDeleteBoardGear rejected: simulation is running.");
                    return false;
                }

                Vector2Int pos = node.Position;
                int reward = node.ConfigData.DeleteRewardAmount;

                IGridNode extracted = gridManager.ExtractNode(pos);

                if (extracted == null)
                {
                    extracted = node;
                }
                else
                {
                    if (extracted != node)
                    {
                        gridManager.AddNode(extracted);
                        extracted = node;
                    }
                }

                RemoveGear(extracted);

                eventBus?.Raise(new GearDeletedEvent(pos, reward));
                Debug.Log($"<color=#ff5555>[BoardService]</color> Gear at {pos} DELETED. Reward: {reward}");
                SyncBoardModel();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardService] TryDeleteBoardGear failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos)
        {
            if (node == null)
            {
                return;
            }

            node.SetPosition(originalPos);
            gridManager.AddNode(node);
            SyncBoardModel();
        }

        private void PlaceNodeAt(IGridNode node, Vector2Int toPos)
        {
            node.SetPosition(toPos);
            gridManager.AddNode(node);
        }

        private void SwapBoardGears(IGridNode draggedNode, IGridNode occupantNode)
        {
            if (swapService != null)
            {
                swapService.SwapNodes(draggedNode, occupantNode);
            }
            else
            {
                Debug.LogError("[BoardService] IGridSwapService is missing!");
                return;
            }

            PlaceGear(occupantNode, draggedNode);
        }

        private void MergeBoardGearsAt(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos, GearConfigData occupantData, Vector2Int fromPos)
        {
            if (mergeService == null)
            {
                Debug.LogError("[BoardService] IGridMergeService is missing!");
                return;
            }

            draggedNode.SetPosition(fromPos);
            gridManager.AddNode(draggedNode);

            IGridNode newNode = mergeService.MergeNodes(draggedNode, occupantNode, targetDropPos);
            RemoveGear(draggedNode, occupantNode);
            if (newNode != null)
            {
                PlaceGear(newNode);
                Debug.Log($"<color=#ffaa55>[BoardService]</color> MERGED board gears into {newNode.ConfigData.Id} at {targetDropPos}!");
            }
        }

        private void PlaceGear(params IGridNode[] nodes)
        {
            foreach (IGridNode node in nodes)
            {
                GearPlaced?.Invoke(node);
            }
        }

        private void RemoveGear(params IGridNode[] nodes)
        {
            foreach (IGridNode node in nodes)
            {
                GearRemoved?.Invoke(node);
                node?.Dispose();
            }
        }

        private void SyncBoardModel()
        {
            boardModel.Nodes.Clear();
            foreach (IGridNode node in gridManager.GetAllNodes())
            {
                boardModel.Nodes.Add(node);
            }

            boardModel.IsSimulationRunning = gridManager.IsRunning;
        }
    }
}
