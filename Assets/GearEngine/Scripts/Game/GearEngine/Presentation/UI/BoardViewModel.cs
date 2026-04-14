using System;
using System.Collections.Generic;
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
        public IGearEngineService EngineService => engineService;
        public BoardConfigSO BoardConfig => boardConfig;

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

        public void Initialize(IGearEngineService engineService, IGridManager gridManager, GearNodeFactory nodeFactory, BoardConfigSO boardConfig, IEventBus eventBus = null, GearEngineFeatureToggleSO featureToggle = null)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.eventBus = eventBus;
            this.featureToggle = featureToggle;
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

            if (gridManager == null || nodeFactory == null || boardConfig == null)
            {
                throw new InvalidOperationException("BoardViewModel must be initialized before LoadLayout.");
            }

            foreach (BoardGearPlacementData placement in layout.Placements)
            {
                TryAddLayoutPlacement(placement);
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

        public bool DeleteGear(IGridNode node)
        {
            try
            {
                return TryDeleteGearInternal(node);
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
            OnBoardDragEnded?.Invoke();
        }

        public bool HandleInventoryDrop(Vector3 worldPosition, GearConfigData gearData)
        {
            try
            {
                return TryHandleInventoryDrop(worldPosition, gearData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] HandleInventoryDrop failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

               private void TryAddLayoutPlacement(BoardGearPlacementData placement)
        {
            if (placement == null || placement.GearConfig == null)
            {
                return;
            }

            Vector2Int pos = placement.Position;
            if (!IsLayoutPositionValid(pos))
            {
                return;
            }

            GearConfigData runtimeData = placement.GearConfig.CreateRuntimeData();
            IGridNode node = nodeFactory.CreateNode(pos, runtimeData);
            gridManager.AddNode(node);
        }

        private bool IsLayoutPositionValid(Vector2Int pos)
        {
            bool inBounds = pos.x >= 0 && pos.x < boardConfig.GridWidth && pos.y >= 0 && pos.y < boardConfig.GridHeight;
            if (!inBounds)
            {
                Debug.LogError($"[BoardViewModel] Ignoring out-of-bounds starting gear at {pos}.");
                return false;
            }

            if (gridManager.GetNode(pos) != null)
            {
                Debug.LogError($"[BoardViewModel] Duplicate starting gear at {pos}.");
                return false;
            }

            return true;
        }

        private void OnGearDroppedInternal(IGridNode node, Vector2Int toPos)
        {
            if (!ValidateDropContext(node) || engineService.IsRunning)
            {
                return;
            }

            if (!IsInsideGrid(toPos))
            {
                RejectDropOutOfBounds(node, toPos);
                return;
            }

            TryPlaceOrResolveOccupied(node, toPos);
        }

        private void RejectDropOutOfBounds(IGridNode node, Vector2Int toPos)
        {
            SnapNodeBackToOriginal(node);
            Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> Drop at {toPos} out of bounds. Snapped back.");
        }

        private void TryPlaceOrResolveOccupied(IGridNode node, Vector2Int toPos)
        {
            IGridNode occupant = gridManager.GetNode(toPos);
            if (occupant == null)
            {
                PlaceNodeAt(node, toPos);
                Debug.Log($"<color=#55ff55>[BoardViewModel]</color> Successfully dropped gear into {toPos}");
                return;
            }

            TryResolveOccupiedDrop(node, occupant, toPos);
        }

        private bool IsInsideGrid(Vector2Int toPos)
        {
            return toPos.x >= 0 && toPos.x < boardConfig.GridWidth && toPos.y >= 0 && toPos.y < boardConfig.GridHeight;
        }

        private bool ValidateDropContext(IGridNode node)
        {
            return node != null && engineService != null && gridManager != null && boardConfig != null;
        }

        private void TryResolveOccupiedDrop(IGridNode node, IGridNode occupant, Vector2Int toPos)
        {
            GearConfigData draggedData = node.ConfigData;
            GearConfigData occupantData = occupant.ConfigData;

            if ((occupantData != null && !occupantData.IsMovable) || (draggedData != null && !draggedData.IsMovable))
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

        private bool TryDeleteGearInternal(IGridNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!CanDeleteGear(node))
            {
                return false;
            }

            return ExtractAndFinalizeDeletion(node);
        }

        private bool CanDeleteGear(IGridNode node)
        {
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

            return engineService == null || !engineService.IsRunning || LogDeleteRejectedBecauseRunning();
        }

        private bool LogDeleteRejectedBecauseRunning()
        {
            Debug.LogWarning($"[BoardViewModel] DeleteGear rejected: simulation is running.");
            return false;
        }

        private bool ExtractAndFinalizeDeletion(IGridNode node)
        {
            Vector2Int pos = node.Position;
            int reward = node.ConfigData.DeleteRewardAmount;
            IGridNode extracted = NormalizeExtractedNode(gridManager.ExtractNode(pos), node);

            OnGearRemoved?.Invoke(extracted);
            extracted.Dispose();
            eventBus?.Raise(new GearDeletedEvent(pos, reward));
            Debug.Log($"<color=#ff5555>[BoardViewModel]</color> Gear at {pos} DELETED. Reward: {reward}");
            return true;
        }

        private IGridNode NormalizeExtractedNode(IGridNode extracted, IGridNode node)
        {
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

        private bool TryHandleInventoryDrop(Vector3 worldPosition, GearConfigData gearData)
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
                return PlaceNewInventoryNode(targetDropPos, gearData);
            }

            return TryMergeOrRejectInventory(occupant, targetDropPos, gearData);
        }

        private bool PlaceNewInventoryNode(Vector2Int targetDropPos, GearConfigData gearData)
        {
            IGridNode newNode = nodeFactory.CreateNode(targetDropPos, gearData);
            gridManager.AddNode(newNode);
            OnGearPlaced?.Invoke(newNode);
            return true;
        }

        private bool TryMergeOrRejectInventory(IGridNode occupant, Vector2Int targetDropPos, GearConfigData gearData)
        {
            GearConfigData occupantData = occupant.ConfigData;

            if (occupantData.Id == gearData.Id && occupantData.NextLevelConfig != null)
            {
                return MergeInventoryOntoOccupant(occupant, targetDropPos, occupantData, gearData);
            }

            Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> UI Drop Cancelled! {gearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
            return false;
        }

        private bool MergeInventoryOntoOccupant(IGridNode occupant, Vector2Int targetDropPos, GearConfigData occupantData, GearConfigData gearData)
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
