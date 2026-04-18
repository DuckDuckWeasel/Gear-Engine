using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Events;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class BoardViewModel : ViewModel
    {
        public IGearEngineService EngineService
        {
            get
            {
                return engineService;
            }
        }

        public BoardConfigSO BoardConfig
        {
            get
            {
                return boardConfig;
            }
        }

        public int CurrentBoardGearCount
        {
            get
            {
                return gridManager?.GetAllNodes().Count() ?? 0;
            }
        }

        public int MaxAllowedBoardGears
        {
            get
            {
                return boardConfig != null ? boardConfig.MaxAllowedBoardGears : int.MaxValue;
            }
        }

        private IGearEngineService engineService;
        private IGridManager gridManager;
        private GearNodeFactory nodeFactory;
        private BoardConfigSO boardConfig;
        private IEventBus eventBus;
        private GearEngineFeatureToggleSO featureToggle;
        private IDragService dragService;
        private Vector2Int pickupOriginalPos;

        public event Action<IGridNode> OnGearPlaced;
        public event Action<IGridNode> OnGearRemoved;

        public void Initialize(IGearEngineService engineService, IGridManager gridManager, GearNodeFactory nodeFactory, BoardConfigSO boardConfig, IEventBus eventBus = null, GearEngineFeatureToggleSO featureToggle = null, IDragService dragService = null)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.eventBus = eventBus;
            this.featureToggle = featureToggle;
            this.dragService = dragService;
        }

        public IGridNode GetNode(Vector2Int coord)
        {
            return gridManager.GetNode(coord);
        }

        public IEnumerable<IGridNode> GetCurrentNodes()
        {
            return gridManager.GetAllNodes();
        }

        public void LoadLayout(BoardLayoutData layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            EnsureLayoutDependencies();
            foreach (BoardGearPlacementData placement in layout.Placements)
            {
                TryLoadPlacement(placement);
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
            if (!CanHandleBoardDrop(node) || engineService.IsRunning)
            {
                return;
            }

            if (!IsInBounds(toPos))
            {
                RejectOutOfBoundsDrop(node, toPos);
                return;
            }

            HandleOccupiedDrop(node, toPos, gridManager.GetNode(toPos));
        }

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
                dragService?.EndDrag();
            }
        }

        public bool DeleteGear(IGridNode node)
        {
            try
            {
                return TryDeleteGear(node);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] DeleteGear failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public void GrantTrashReward(int rewardAmount)
        {
            if (rewardAmount > 0)
            {
                eventBus?.Raise(new GearDeletedEvent(Vector2Int.zero, rewardAmount));
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

        public bool HandleInventoryDrop(Vector3 worldPosition, GearConfigData gearData)
        {
            try
            {
                return TryHandleInventoryDropInternal(worldPosition, gearData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] HandleInventoryDrop failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private void EnsureLayoutDependencies()
        {
            if (gridManager == null || nodeFactory == null || boardConfig == null)
            {
                throw new InvalidOperationException("BoardViewModel must be initialized before LoadLayout.");
            }
        }

        private void TryLoadPlacement(BoardGearPlacementData placement)
        {
            if (placement == null || placement.GearConfig == null)
            {
                return;
            }

            Vector2Int pos = placement.Position;
            if (!CanPlaceInitialGear(pos))
            {
                return;
            }

            CreateAndAddNode(pos, placement.GearConfig.CreateRuntimeData());
        }

        private bool CanHandleBoardDrop(IGridNode node)
        {
            return node != null && engineService != null && gridManager != null && boardConfig != null;
        }

        private void RejectOutOfBoundsDrop(IGridNode node, Vector2Int toPos)
        {
            SnapNodeBackToOriginal(node);
            Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> Drop at {toPos} out of bounds. Snapped back.");
        }

        private bool TryDeleteGear(IGridNode node)
        {
            EnsureDeleteTarget(node);
            if (!CanDeleteConfig(node) || !IsTrashDeletionEnabled() || !IsSimulationStopped())
            {
                return false;
            }

            IGridNode extracted = ExtractNodeForDeletion(node);
            CompleteDeletion(extracted);
            return true;
        }

        private IGridNode ExtractNodeForDeletion(IGridNode node)
        {
            IGridNode extracted = gridManager.ExtractNode(node.Position);
            if (extracted == null)
            {
                return node;
            }

            if (extracted != node)
            {
                gridManager.AddNode(extracted);
                return node;
            }

            return extracted;
        }

        private void CompleteDeletion(IGridNode extracted)
        {
            Vector2Int pos = extracted.Position;
            int reward = extracted.ConfigData.DeleteRewardAmount;
            OnGearRemoved?.Invoke(extracted);
            extracted.Dispose();
            eventBus?.Raise(new GearDeletedEvent(pos, reward));
            Debug.Log($"<color=#ff5555>[BoardViewModel]</color> Gear at {pos} DELETED. Reward: {reward}");
        }

        private bool TryHandleInventoryDropInternal(Vector3 worldPosition, GearConfigData gearData)
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
                return TryPlaceInventoryGear(targetDropPos, gearData);
            }

            return TryMergeInventoryGear(targetDropPos, gearData, occupant);
        }

        private bool TryPlaceInventoryGear(Vector2Int targetDropPos, GearConfigData gearData)
        {
            if (CurrentBoardGearCount >= boardConfig.MaxAllowedBoardGears)
            {
                Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> Board limit reached ({CurrentBoardGearCount}/{boardConfig.MaxAllowedBoardGears}). Cannot place gear.");
                return false;
            }

            CreateAndAddNode(targetDropPos, gearData);
            return true;
        }

        private bool TryMergeInventoryGear(Vector2Int targetDropPos, GearConfigData gearData, IGridNode occupant)
        {
            GearConfigData occupantData = occupant.ConfigData;
            if (!ShouldMerge(occupantData, gearData))
            {
                Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> UI Drop Cancelled! {gearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
                return false;
            }

            return ReplaceMergedInventoryNode(targetDropPos, occupant, occupantData);
        }

        private bool ShouldMerge(GearConfigData occupantData, GearConfigData draggedData)
        {
            return occupantData.Id == draggedData.Id && occupantData.NextLevelConfig != null;
        }

        private bool ReplaceMergedInventoryNode(Vector2Int targetDropPos, IGridNode occupant, GearConfigData occupantData)
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
            CreateAndAddNode(targetDropPos, upgradedData);
            Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED UI {occupantData.Id} into {upgradedData.Id} at {targetDropPos}!");
            return true;
        }

        private IGridNode CreateAndAddNode(Vector2Int position, GearConfigData configData)
        {
            IGridNode newNode = nodeFactory.CreateNode(position, configData);
            gridManager.AddNode(newNode);
            OnGearPlaced?.Invoke(newNode);
            return newNode;
        }

        private bool CanPlaceInitialGear(Vector2Int pos)
        {
            if (!IsInBounds(pos))
            {
                Debug.LogError($"[BoardViewModel] Ignoring out-of-bounds starting gear at {pos}.");
                return false;
            }

            if (gridManager.GetNode(pos) == null)
            {
                return true;
            }

            Debug.LogError($"[BoardViewModel] Duplicate starting gear at {pos}.");
            return false;
        }

        private void HandleOccupiedDrop(IGridNode node, Vector2Int toPos, IGridNode occupant)
        {
            if (TryPlaceIntoEmptySlot(node, toPos, occupant))
            {
                return;
            }

            if (!CanSwapWithOccupant(node, occupant, toPos))
            {
                return;
            }

            ProcessOccupiedDrop(node, occupant, toPos);
        }

        private bool CanDeleteConfig(IGridNode node)
        {
            if (node.ConfigData != null && node.ConfigData.IsDeletable)
            {
                return true;
            }

            Debug.LogWarning("[BoardViewModel] DeleteGear rejected: gear is not deletable.");
            return false;
        }

        private bool IsTrashDeletionEnabled()
        {
            if (featureToggle == null || featureToggle.EnableTrashDeletion)
            {
                return true;
            }

            Debug.LogWarning("[BoardViewModel] DeleteGear rejected: trash deletion feature is disabled.");
            return false;
        }

        private bool IsSimulationStopped()
        {
            if (engineService == null || !engineService.IsRunning)
            {
                return true;
            }

            Debug.LogWarning("[BoardViewModel] DeleteGear rejected: simulation is running.");
            return false;
        }

        private void EnsureDeleteTarget(IGridNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
        }

        private bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < boardConfig.GridWidth && pos.y >= 0 && pos.y < boardConfig.GridHeight;
        }

        private bool TryPlaceIntoEmptySlot(IGridNode node, Vector2Int toPos, IGridNode occupant)
        {
            if (occupant != null)
            {
                return false;
            }

            ((NodeBase)node).Position = toPos;
            gridManager.AddNode(node);
            OnGearPlaced?.Invoke(node);
            Debug.Log($"<color=#55ff55>[BoardViewModel]</color> Successfully dropped gear into {toPos}");
            return true;
        }

        private bool CanSwapWithOccupant(IGridNode node, IGridNode occupant, Vector2Int toPos)
        {
            GearConfigData draggedData = node.ConfigData;
            GearConfigData occupantData = occupant.ConfigData;
            if ((occupantData != null && !occupantData.IsMovable) || (draggedData != null && !draggedData.IsMovable))
            {
                SnapNodeBackToOriginal(node);
                Debug.Log($"<color=#ff5555>[BoardViewModel]</color> Cannot swap '{draggedData?.Id}' with '{occupantData?.Id}' at {toPos} - at least one gear is not movable. Snapped back.");
                return false;
            }

            return true;
        }

        private void ProcessOccupiedDrop(IGridNode node, IGridNode occupant, Vector2Int toPos)
        {
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

        private void SnapNodeBackToOriginal(IGridNode node)
        {
            ((NodeBase)node).Position = pickupOriginalPos;
            gridManager.AddNode(node);
            OnGearPlaced?.Invoke(node);
        }

        private void SwapBoardGears(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos)
        {
            gridManager.ExtractNode(targetDropPos);

            // Place dragged node at target position
            ((NodeBase)draggedNode).Position = targetDropPos;
            gridManager.AddNode(draggedNode);

            // Place occupant at the original pickup position
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
