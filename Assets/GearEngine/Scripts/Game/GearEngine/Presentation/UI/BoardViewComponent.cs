using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Assertions;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardViewComponent : ViewComponent<BoardViewModel>
    {
        [SerializeField] private GearBoardDragHandler dragHandler;
        [SerializeField] private GameObject gridSlotPrefab;
        [SerializeField] private Transform gridRoot;

        private GearViewFactory localFactory;
        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();
        private readonly List<GameObject> backgroundSlots = new List<GameObject>();

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[BoardView] ViewModel is missing.");
            Assert.IsNotNull(dragHandler, "[BoardView] DragHandler is not assigned.");
            Assert.IsNotNull(gridSlotPrefab, "[BoardView] GridSlotPrefab is not assigned.");
            Assert.IsNotNull(gridRoot, "[BoardView] GridRoot is not assigned.");

            localFactory = new GearViewFactory();

            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;

            SpawnBackgroundGrid(viewModel.BoardConfig);
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }

            Bind(() => viewModel.Interactable, () => dragHandler.enabled);
        }

        protected override void OnUnbind()
        {
            if (viewModel != null)
            {
                viewModel.OnGearPlaced -= HandleGearPlaced;
                viewModel.OnGearRemoved -= HandleGearRemoved;
            }

            DestroyAllViews();
            localFactory = null;

            if (dragHandler != null)
            {
                dragHandler.enabled = false;
            }

            base.OnUnbind();
        }

        internal void NotifyPickedUp(IGridNode node, Vector2Int coord)
        {
            viewModel?.OnGearPickedUp(node, coord);
        }

        internal void NotifyDropped(IGridNode node, Vector2Int coord)
        {
            viewModel?.OnGearDropped(node, coord);
        }

        internal void NotifyBoardGearDroppedOverUI(IGridNode node, GearConfigData config, Vector3 worldPos)
        {
            viewModel?.HandleBoardGearReturnedOverUI(node, config);
        }

        internal void NotifyTrashDrop(IGridNode node)
        {
            viewModel?.RequestTrashDrop(node);
        }

        internal GearView DetachViewForDrag(IGridNode node)
        {
            if (node == null)
            {
                return null;
            }

            if (viewsByNode.TryGetValue(node, out GearView view))
            {
                viewsByNode.Remove(node);
                return view;
            }

            return null;
        }

        internal void DestroyDetachedView(GearView view)
        {
            if (view != null)
            {
                DestroyViewGameObject(view.gameObject);
            }
        }

        internal IEnumerable<GearView> GetViews()
        {
            return viewsByNode.Values;
        }

        internal bool IsRunning()
        {
            return viewModel?.EngineService?.IsRunning ?? false;
        }

        internal BoardConfigSO GetBoardConfig()
        {
            return viewModel?.BoardConfig;
        }

        private void HandleGearPlaced(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            SpawnView(node);
        }

        private void HandleGearRemoved(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            if (!viewsByNode.TryGetValue(node, out GearView view))
            {
                Debug.Log($"<color=#ff9900>[BoardView]</color> HandleGearRemoved: no view found for node '{node.ConfigData?.Id}' at {node.Position}.");
                return;
            }

            Debug.Log($"<color=#ff5555>[BoardView]</color> Destroying GearView for '{node.ConfigData?.Id}' at {node.Position}.");
            viewsByNode.Remove(node);
            DestroyViewGameObject(view.gameObject);
        }

        private void SpawnView(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            if (viewsByNode.TryGetValue(node, out GearView existingView))
            {
                return;
            }

            Vector3 localPosition = viewModel.BoardConfig.GetWorldPosition(node.Position);
            GearView view = localFactory.CreateView(node, node.ConfigData, transform, localPosition);
            view.Initialize(node, node.ConfigData, viewModel.BoardConfig, localFactory);
            viewsByNode[node] = view;
        }

        private void DestroyAllViews()
        {
            foreach (KeyValuePair<IGridNode, GearView> pair in viewsByNode)
            {
                if (pair.Value != null)
                {
                    DestroyViewGameObject(pair.Value.gameObject);
                }
            }

            viewsByNode.Clear();

            foreach (GameObject slot in backgroundSlots)
            {
                if (slot != null)
                {
                    DestroyViewGameObject(slot);
                }
            }
            backgroundSlots.Clear();
        }

        private void SpawnBackgroundGrid(BoardConfigSO config)
        {
            Assert.IsNotNull(config, "[BoardView] BoardConfigSO is missing.");

            for (int x = 0; x < config.GridWidth; x++)
            {
                for (int y = 0; y < config.GridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GameObject slotView = Instantiate(gridSlotPrefab, gridRoot);
                    slotView.transform.localPosition = config.GetWorldPosition(pos, 0.5f);
                    slotView.name = $"GridSlot_{x}_{y}";
                    backgroundSlots.Add(slotView);
                }
            }
        }

        private void DestroyViewGameObject(GameObject go)
        {
            go.SafeDestroy();
        }
    }
}
