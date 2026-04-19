using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardViewComponent : ViewComponent<BoardViewModel>, IDragTarget
    {
        [SerializeField] private GameObject gridSlotPrefab;
        [SerializeField] private Transform gridRoot;
        [Tooltip("Legacy root for board-space plane. Gears parent to grid slots.")]
        [SerializeField] private Transform gearsRoot;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;

        [SerializeField]
        [Tooltip("Layout math for slots, stagger rotation, and drop projection (view-only).")]
        private BoardLayoutSO boardLayout;

        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();
        private readonly Dictionary<Vector2Int, Transform> slotByCoord = new Dictionary<Vector2Int, Transform>();
        private readonly List<GameObject> backgroundSlots = new List<GameObject>();

        public BoardLayoutSO BoardLayout => boardLayout;

        protected override void OnBind()
        {
            Assert.IsNotNull(boardLayout, "[BoardView] BoardLayoutSO is missing.");

            // Board gears are world-space colliders; the EventSystem only dispatches
            // IBeginDragHandler to them via Physics2DRaycaster on the rendering camera.
            EnsureBoardCameraPhysics2DRaycaster();

            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;

            SpawnBackgroundGrid();
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }

            viewModel.PropertyChanged += OnBoardViewModelPropertyChanged;
            RefreshDraggableInteractable();
            Bind(() => viewModel.BoardLimitText, () => boardLimitLabel.text);
        }

        protected override void OnUnbind()
        {
            if (viewModel != null)
            {
                viewModel.OnGearPlaced -= HandleGearPlaced;
                viewModel.OnGearRemoved -= HandleGearRemoved;
                viewModel.PropertyChanged -= OnBoardViewModelPropertyChanged;
            }

            DestroyAllViews();

            base.OnUnbind();
        }

        private void OnBoardViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BoardViewModel.Interactable))
            {
                RefreshDraggableInteractable();
            }
        }

        private void RefreshDraggableInteractable()
        {
            bool boardInteractable = viewModel != null && viewModel.Interactable;
            foreach (KeyValuePair<IGridNode, GearView> pair in viewsByNode)
            {
                Draggable drag = pair.Value != null ? pair.Value.GetComponent<Draggable>() : null;
                if (drag == null)
                {
                    continue;
                }

                IGridNode node = pair.Key;
                bool movable = node != null && node.IsInteractable &&
                                 node.ConfigData != null && node.ConfigData.IsMovable;
                drag.IsInteractable = boardInteractable && movable;
            }
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
            AttachDraggable(view, node);
        }

        private void AttachDraggable(GearView view, IGridNode node)
        {
            GameObject go = view.gameObject;
            if (go.GetComponent<Collider2D>() == null)
            {
                go.AddComponent<BoxCollider2D>();
            }

            Draggable drag = go.GetComponent<Draggable>() ?? go.AddComponent<Draggable>();
            drag.SetHideSourceWhileDragging(true);
            Transform boardRoot = GetBoardSpaceRoot();
            drag.PreviewParent = boardRoot;
            bool boardInteractable = viewModel != null && viewModel.Interactable;
            bool movable = node.IsInteractable && node.ConfigData != null && node.ConfigData.IsMovable;
            drag.IsInteractable = boardInteractable && movable;

            Camera main = Camera.main;
            drag.BuildPayload = e =>
            {
                Vector3 world;
                if (main != null && boardRoot != null &&
                    BoardPointerProjectionUtility.TryProjectScreenPointToPlane(main, e.position, boardRoot, out world))
                {
                    return new DragPayload(node, world);
                }

                world = DragPointerUtility.GetWorldPosition(e);
                return new DragPayload(node, world);
            };

            drag.OnDropAccepted = target => OnGearDragAccepted(node, target);
        }

        private void OnGearDragAccepted(IGridNode node, IDragTarget target)
        {
            switch (target)
            {
                case BoardViewComponent _:
                    break;
                case GearInventoryViewComponent _:
                    if (node?.ConfigData != null)
                    {
                        viewModel.CompleteBoardGearReturnToInventory(node, node.ConfigData);
                    }

                    break;
            }
        }

        private static void EnsureBoardCameraPhysics2DRaycaster()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Type raycasterType = Type.GetType("UnityEngine.UI.Physics2DRaycaster, UnityEngine.UI");
            if (raycasterType != null && cam.GetComponent(raycasterType) == null)
            {
                cam.gameObject.AddComponent(raycasterType);
            }
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

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<GearConfigData>() != null || payload.GetData<IGridNode>() != null;
        }

        public bool OnDrop(DragPayload payload)
        {
            GearConfigData gear = payload.GetData<GearConfigData>();
            IGridNode draggedNode = payload.GetData<IGridNode>();
            if (viewModel == null || boardLayout == null)
            {
                return false;
            }

            Vector3 boardLocal = GetBoardSpaceRoot().InverseTransformPoint(payload.WorldPosition);
            Vector2Int gridPos = boardLayout.GetGridPosition(boardLocal, viewModel.BoardRules);

            if (draggedNode != null)
            {
                return viewModel.TryMoveBoardGear(draggedNode, gridPos);
            }

            if (gear != null)
            {
                return viewModel.HandleInventoryDrop(gridPos, gear);
            }

            return false;
        }
    }
}
