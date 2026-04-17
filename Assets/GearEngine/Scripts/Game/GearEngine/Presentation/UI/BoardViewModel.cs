using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class BoardViewModel : ViewModel
    {
        public BoardViewModel(IGearEngineService engineService, IGridManager gridManager, IGearNodeFactory nodeFactory, BoardConfigSO boardConfig, IGearPresentationTransferService presentationTransfer, IEventBus eventBus = null, GearEngineFeatureToggleSO featureToggle = null, IDragService dragService = null, IGridSwapService swapService = null, IGridMergeService mergeService = null, BoardLayoutData initialLayout = null)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.presentationTransfer = presentationTransfer ?? throw new ArgumentNullException(nameof(presentationTransfer));
            this.eventBus = eventBus;
            this.featureToggle = featureToggle;
            this.dragService = dragService;
            this.swapService = swapService;
            this.mergeService = mergeService;

            RefreshSimulationRunningFromGrid();

            if (initialLayout != null)
            {
                LoadLayout(initialLayout);
            }

            UpdateLabels();
        }

        public IGearEngineService EngineService => engineService;
        public BoardConfigSO BoardConfig => boardConfig;
        public int CurrentBoardGearCount => gridManager?.GetAllNodes().Count() ?? 0;
        public int MaxAllowedBoardGears => boardConfig != null ? boardConfig.MaxAllowedBoardGears : int.MaxValue;

        private readonly IGearEngineService engineService;
        private readonly IGridManager gridManager;
        private readonly IGearNodeFactory nodeFactory;
        private readonly BoardConfigSO boardConfig;
        private readonly IGearPresentationTransferService presentationTransfer;
        private readonly IEventBus eventBus;
        private readonly GearEngineFeatureToggleSO featureToggle;
        private readonly IDragService dragService;
        private readonly IGridSwapService swapService;
        private readonly IGridMergeService mergeService;
        private Vector2Int pickupOriginalPos;

        [ObservableProperty] private bool interactable = true;
        [ObservableProperty] private string boardLimitText = string.Empty;
        [ObservableProperty] private bool isSimulationRunning;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;

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

                RefreshSimulationRunningFromGrid();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] ToggleSimulation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshSimulationRunningFromGrid()
        {
            IsSimulationRunning = gridManager != null && gridManager.IsRunning;
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
                bool inBounds = pos.x >= 0 && pos.x < boardConfig.GridWidth && pos.y >= 0 && pos.y < boardConfig.GridHeight;

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
            dragService?.StartDrag(node.ConfigData);
        }

        public void OnGearDropped(IGridNode node, Vector2Int toPos)
        {
            try
            {
                OnGearDroppedInternal(node, toPos);
            }
            finally
            {
                dragService?.EndDrag();
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

            SwapBoardGears(node, occupant);
            Debug.Log($"<color=#ffff33>[BoardViewModel]</color> Swapped positions! {toPos} <-> {pickupOriginalPos}");
        }

        public void CompleteBoardGearReturnToInventory(IGridNode node, GearConfigData config)
        {
            try
            {
                presentationTransfer.AddReturnedBoardGearToInventory(config);
                RemoveGear(node);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] CompleteBoardGearReturnToInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                dragService?.EndDrag();
            }
        }

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
                    // Safety check: if another node somehow occupied this pos, put it back
                    gridManager.AddNode(extracted);
                    extracted = node;
                }

                RemoveGear(extracted);

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

        public void SnapBackToOriginal(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            SnapNodeBackToOriginal(node);
            dragService?.EndDrag();
        }

        public bool HandleInventoryDrop(Vector2Int targetDropPos, GearConfigData gearData)
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

                IGridNode occupant = gridManager.GetNode(targetDropPos);

                if (occupant == null)
                {
                    // Check board limit before placing
                    if (CurrentBoardGearCount >= boardConfig.MaxAllowedBoardGears)
                    {
                        Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> Board limit reached ({CurrentBoardGearCount}/{boardConfig.MaxAllowedBoardGears}). Cannot place gear.");
                        return false;
                    }

                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, gearData);
                    gridManager.AddNode(newNode);
                    PlaceGear(newNode);
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

                    RemoveGear(occupant);

                    GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
                    gridManager.AddNode(newNode);
                    PlaceGear(newNode);
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
            node.SetPosition(toPos);
            gridManager.AddNode(node);
        }

        private void SnapNodeBackToOriginal(IGridNode node)
        {
            node.SetPosition(pickupOriginalPos);
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
                Debug.LogError("[BoardViewModel] IGridSwapService is missing!");
                return;
            }
            
            PlaceGear(occupantNode, draggedNode);
        }

        private void MergeBoardGearsAt(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos, GearConfigData occupantData)
        {
            if (mergeService == null)
            {
                Debug.LogError("[BoardViewModel] IGridMergeService is missing!");
                return;
            }

            // Restore the node to grid temporarily so the merge service can safely extract it natively
            draggedNode.SetPosition(pickupOriginalPos);
            gridManager.AddNode(draggedNode);

            IGridNode newNode = mergeService.MergeNodes(draggedNode, occupantNode, targetDropPos);
            RemoveGear(draggedNode, occupantNode);
            if (newNode != null)
            {
                PlaceGear(newNode);
                Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED board gears into {newNode.ConfigData.Id} at {targetDropPos}!");
            }
        }

        private void PlaceGear(params IGridNode[] nodes)
        {
            foreach(var node in nodes)
            {
                OnGearPlaced?.Invoke(node);
            }
            UpdateLabels();
        }

        private void RemoveGear(params IGridNode[] nodes)
        {
            foreach(var node in nodes)
            {
                OnGearRemoved?.Invoke(node);
                node?.Dispose();
            }
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            BoardLimitText = $"Board: {CurrentBoardGearCount}/{MaxAllowedBoardGears}";
        }
    }
}
