using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardViewComponent : ViewComponent<BoardViewModel>, IDragTarget
    {
        [SerializeField] private GearBoardDragHandler dragHandler;
        [SerializeField] private GameObject gridSlotPrefab;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;

        private GearViewFactory localFactory = new GearViewFactory();
        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();
        private readonly List<GameObject> backgroundSlots = new List<GameObject>();

        public new void Unbind()
        {
            base.Unbind();
        }

        protected override void OnBind()
        {
            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;

            SpawnBackgroundGrid(viewModel.BoardConfig);
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }

            Bind(() => viewModel.Interactable, () => dragHandler.enabled);
            Bind(() => viewModel.BoardLimitText, () => boardLimitLabel.text);
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

        internal void NotifyBoardGearReturnAccepted(IGridNode node, GearConfigData config, Vector3 worldPos)
        {
            viewModel?.CompleteBoardGearReturnToInventory(node, config);
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

        public void OnDragStarted(DragPayload payload)
        {
        }

        public void OnDragEnded()
        {
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<GearConfigData>() != null;
        }

        public void OnDrop(DragPayload payload)
        {
            GearConfigData gear = payload.GetData<GearConfigData>();
            if (gear == null || viewModel == null)
            {
                return;
            }

            Vector3 localWorld = payload.WorldPosition - transform.position;
            Vector2Int gridPos = viewModel.BoardConfig.GetGridPosition(localWorld);
            bool placed = viewModel.HandleInventoryDrop(gridPos, gear);
            if (placed)
            {
                payload.Source?.OnDropAccepted(this);
            }
        }

        public void OnHoverEnter(DragPayload payload)
        {
        }

        public void OnHoverExit()
        {
        }
    }
}
