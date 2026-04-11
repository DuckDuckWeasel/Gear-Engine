using System;
using Game.GearEngine;
using Game.GearEngine.Events;
using Scaffold.Events;
using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public sealed class BoardViewModel : ViewModel, IDisposable
    {
        private IGearEngineService engineService;
        private IGridManager gridManager;
        private GearNodeFactory nodeFactory;
        private GearViewFactory viewFactory;
        private GearInventoryViewModel inventoryViewModel;
        private BoardConfigSO boardConfig;
        private EventController eventController;

        private Vector2Int pickupOriginalPos;
        private Transform boardVisualRoot;
        private bool eventSubscribed;

        public void SetBoardVisualRoot(Transform root)
        {
            boardVisualRoot = root;
        }

        public void Initialize(
            IGearEngineService engineService,
            IGridManager gridManager,
            GearNodeFactory nodeFactory,
            GearViewFactory viewFactory,
            GearInventoryViewModel inventory,
            BoardConfigSO boardConfig,
            EventController eventController)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.gridManager = gridManager ?? throw new ArgumentNullException(nameof(gridManager));
            this.nodeFactory = nodeFactory ?? throw new ArgumentNullException(nameof(nodeFactory));
            this.viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            inventoryViewModel = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.eventController = eventController ?? throw new ArgumentNullException(nameof(eventController));

            this.eventController.AddListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
            eventSubscribed = true;
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

        public void OnGearDropped(IGridNode node, Vector2Int toPos, bool isOverUI)
        {
            if (node == null || engineService == null || gridManager == null || boardConfig == null)
            {
                return;
            }

            if (engineService.IsRunning)
            {
                return;
            }

            if (isOverUI)
            {
                ReturnGearToInventoryFromBoard(node);
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

            var draggedData = ((NodeBase)node).ConfigData;
            var occupantData = ((NodeBase)occupant).ConfigData;

            if (occupantData.Id == draggedData.Id && occupantData.NextLevelConfig != null)
            {
                MergeBoardGearsAt(node, occupant, toPos, occupantData);
                return;
            }

            SwapBoardGears(node, occupant, toPos);
            Debug.Log($"<color=#ffff33>[BoardViewModel]</color> Swapped positions! {toPos} <-> {pickupOriginalPos}");
        }

        private void PlaceNodeAt(IGridNode node, Vector2Int toPos)
        {
            ((NodeBase)node).Position = toPos;
            gridManager.AddNode(node);
            viewFactory.GetView(node)?.RecalculateRotationOffset();
        }

        private void SnapNodeBackToOriginal(IGridNode node)
        {
            ((NodeBase)node).Position = pickupOriginalPos;
            gridManager.AddNode(node);
            viewFactory.GetView(node)?.RecalculateRotationOffset();
        }

        private void SwapBoardGears(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos)
        {
            gridManager.ExtractNode(targetDropPos);

            ((NodeBase)draggedNode).Position = targetDropPos;
            gridManager.AddNode(draggedNode);
            viewFactory.GetView(draggedNode)?.RecalculateRotationOffset();

            ((NodeBase)occupantNode).Position = pickupOriginalPos;
            gridManager.AddNode(occupantNode);

            GearView occupantView = viewFactory.GetView(occupantNode);
            occupantView?.RecalculateRotationOffset();
        }

        private void MergeBoardGearsAt(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetDropPos, GearConfigData occupantData)
        {
            gridManager.ExtractNode(targetDropPos);

            DestroyGearViewForNode(draggedNode);
            DestroyGearViewForNode(occupantNode);

            draggedNode.Dispose();
            occupantNode.Dispose();

            GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
            IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
            gridManager.AddNode(newNode);
            Transform parent = boardVisualRoot != null ? boardVisualRoot : null;
            if (parent == null)
            {
                Debug.LogError("[BoardViewModel] Board visual root is not set; cannot spawn merged gear view.");
                return;
            }

            viewFactory.CreateView(newNode, upgradedData, parent);
            Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED board gears into {upgradedData.Id} at {targetDropPos}!");
        }

        private void DestroyGearViewForNode(IGridNode node)
        {
            GearView view = viewFactory.GetView(node);
            if (view != null)
            {
                viewFactory.UnregisterView(node);
                DestroyViewGameObject(view.gameObject);
            }
        }

        private void ReturnGearToInventoryFromBoard(IGridNode node)
        {
            GearConfigData draggedData = ((NodeBase)node).ConfigData;
            inventoryViewModel.AddGearToInventory(draggedData);
            GearView v = viewFactory.GetView(node);
            if (v != null)
            {
                viewFactory.UnregisterView(node);
                DestroyViewGameObject(v.gameObject);
            }

            node.Dispose();
        }

        private static void DestroyViewGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
                return;
            }
#endif
            UnityEngine.Object.Destroy(go);
        }

        private void HandleGearDroppedFromUI(GearDroppedFromUIEvent context)
        {
            try
            {
                if (gridManager == null || boardConfig == null || engineService == null || engineService.IsRunning)
                {
                    return;
                }

                Vector2Int targetDropPos = boardConfig.GetGridPosition(context.WorldPosition);

                IGridNode occupant = gridManager.GetNode(targetDropPos);

                if (occupant == null)
                {
                    bool consumed = inventoryViewModel.ConsumeSpecificGear(context.GearData);
                    if (consumed)
                    {
                        IGridNode newNode = nodeFactory.CreateNode(targetDropPos, context.GearData);
                        gridManager.AddNode(newNode);
                        Transform parent = boardVisualRoot;
                        if (parent == null)
                        {
                            Debug.LogError("[BoardViewModel] Board visual root is not set; cannot spawn gear from UI drop.");
                            return;
                        }

                        viewFactory.CreateView(newNode, context.GearData, parent);
                    }
                }
                else
                {
                    GearConfigData occupantData = ((NodeBase)occupant).ConfigData;

                    if (occupantData.Id == context.GearData.Id && occupantData.NextLevelConfig != null)
                    {
                        bool consumed = inventoryViewModel.ConsumeSpecificGear(context.GearData);
                        if (consumed)
                        {
                            IGridNode removedOccupant = gridManager.ExtractNode(targetDropPos);
                            if (removedOccupant != occupant)
                            {
                                Debug.LogError("[BoardViewModel] Grid state mismatch during UI merge.");
                                return;
                            }

                            DestroyGearViewForNode(occupant);
                            occupant.Dispose();

                            GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
                            IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
                            gridManager.AddNode(newNode);
                            Transform parent = boardVisualRoot;
                            if (parent == null)
                            {
                                Debug.LogError("[BoardViewModel] Board visual root is not set; cannot spawn merged gear from UI.");
                                return;
                            }

                            viewFactory.CreateView(newNode, upgradedData, parent);
                            Debug.Log($"<color=#ffaa55>[BoardViewModel]</color> MERGED UI {context.GearData.Id} into {upgradedData.Id} at {targetDropPos}!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"<color=#ff5555>[BoardViewModel]</color> UI Drop Cancelled! {context.GearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardViewModel] HandleGearDroppedFromUI failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void Dispose()
        {
            if (eventController != null && eventSubscribed)
            {
                eventController.RemoveListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
                eventSubscribed = false;
            }
        }
    }
}
