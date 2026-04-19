using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Nodes;
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
        [Tooltip("Legacy root for board-space plane / drag ghost. Gears parent to grid slots.")]
        [SerializeField] private Transform gearsRoot;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;

        [SerializeField]
        [Tooltip("Layout math for slots, stagger rotation, and drop projection (view-only).")]
        private BoardLayoutSO boardLayout;

        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();
        private readonly Dictionary<Vector2Int, Transform> slotByCoord = new Dictionary<Vector2Int, Transform>();
        private readonly List<GameObject> backgroundSlots = new List<GameObject>();

        internal BoardLayoutSO BoardLayout => boardLayout;

        protected override void OnBind()
        {
            Assert.IsNotNull(boardLayout, "[BoardView] BoardLayoutSO is missing.");

            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;

            SpawnBackgroundGrid();
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

        /// <summary>Transform whose local space matches <see cref="BoardLayoutSO.GetGridPosition"/> / grid layout.</summary>
        public Transform GetBoardSpaceRoot()
        {
            if (gearsRoot != null)
            {
                return gearsRoot;
            }

            if (gridRoot != null)
            {
                return gridRoot;
            }

            return transform;
        }

        internal Vector2Int BoardLocalToGrid(Vector3 boardLocal)
        {
            if (boardLayout == null || viewModel == null)
            {
                return Vector2Int.zero;
            }

            return boardLayout.GetGridPosition(boardLocal, viewModel.BoardRules);
        }

        private Transform GetSlotTransform(Vector2Int pos)
        {
            return slotByCoord.TryGetValue(pos, out Transform t) ? t : null;
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

            Transform slot = GetSlotTransform(node.Position);
            GearView prefab = node.ConfigData?.ViewPrefab;
            if (slot == null || prefab == null)
            {
                Debug.LogError($"[BoardView] Cannot spawn gear: slot or ViewPrefab missing for '{node.ConfigData?.Id}' at {node.Position}.");
                return;
            }

            GearView view = Instantiate(prefab, slot, false);
            view.Bind(node, boardLayout, viewModel.BoardRules, GetSlotTransform, node.ConfigData);
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
            slotByCoord.Clear();

            foreach (GameObject slot in backgroundSlots)
            {
                if (slot != null)
                {
                    DestroyViewGameObject(slot);
                }
            }

            backgroundSlots.Clear();
        }

        private void SpawnBackgroundGrid()
        {
            BoardRulesSO rules = viewModel.BoardRules;
            Assert.IsNotNull(rules, "[BoardView] BoardRulesSO is missing.");

            for (int x = 0; x < rules.GridWidth; x++)
            {
                for (int y = 0; y < rules.GridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GameObject slotView = Instantiate(gridSlotPrefab, gridRoot, false);
                    slotView.transform.localPosition = boardLayout.GetCellLocalPosition(pos, rules, 0.5f);
                    slotView.name = $"GridSlot_{x}_{y}";
                    slotByCoord[pos] = slotView.transform;
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
            if (gear == null || viewModel == null || boardLayout == null)
            {
                return;
            }

            Vector3 boardLocal = GetBoardSpaceRoot().InverseTransformPoint(payload.WorldPosition);
            Vector2Int gridPos = boardLayout.GetGridPosition(boardLocal, viewModel.BoardRules);
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
