using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Ami.BroAudio;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardViewComponent : ViewComponent<BoardViewModel>, IDragTarget
    {
        public BoardLayoutSO BoardLayout => boardLayout;

        [SerializeField] private GameObject gridSlotPrefab;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private RectTransform gearsRoot;
        [SerializeField] private TextMeshProUGUI boardLimitLabel;
        [Tooltip("GameObjects to enable when gears are present, and disable when empty.")]
        [SerializeField] private List<GameObject> activeGridVisuals = new List<GameObject>();

        [SerializeField]
        [Tooltip("Layout math for slots, stagger rotation, and drop projection (view-only).")]
        private BoardLayoutSO boardLayout;

        [SerializeField]
        private BoardGearAnimator animator;

        [SerializeField]
        [Tooltip("Prefab for floating combat text when a gear is triggered.")]
        private FloatingText floatingTextPrefab;

        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();
        private readonly Dictionary<Vector2Int, Transform> slotByCoord = new Dictionary<Vector2Int, Transform>();
        private readonly List<GameObject> backgroundSlots = new List<GameObject>();
        private bool isRapidSpinningAll;
        private bool workspaceInteractionEnabled = true;
        private IDragService dragService;
        private RectTransform dragOverlay;
        private global::GearEngine.CarSimulation.Presentation.CarView cachedCarView;

        public void SetDragContext(IDragService service, RectTransform overlay)
        {
            dragService = service;
            dragOverlay = overlay;
        }

        public void SetWorkspaceInteractionEnabled(bool enabled)
        {
            workspaceInteractionEnabled = enabled;
            RefreshDraggableInteractable();
        }

        public new void Unbind()
        {
            base.Unbind();
        }

        protected override void OnBind()
        {
            Assert.IsNotNull(boardLayout, "[BoardView] BoardLayoutSO is missing.");
            Assert.IsNotNull(animator, "[BoardView] BoardGearAnimator is missing.");
            ConfigureAnimator();
            SubscribeViewModelEvents();
            PopulateBoard();
            Bind<bool, bool>(() => viewModel.Interactable, _ => RefreshDraggableInteractable());
            Bind(() => viewModel.BoardLimitText, () => boardLimitLabel.text);
        }

        private void ConfigureAnimator()
        {
            animator.Configure(GetSlotTransform, boardLayout, viewModel.MotorCogGearId, IsSimulationRunning);
        }

        private bool IsSimulationRunning()
        {
            return viewModel.IsSimulationRunning || (viewModel.EngineService != null && viewModel.EngineService.IsRunning);
        }

        private void SubscribeViewModelEvents()
        {
            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;
            viewModel.OnGearTriggered += HandleGearTriggered;
            viewModel.OnGearChargeCompleted += HandleGearChargeCompleted;
        }

        private void PopulateBoard()
        {
            SpawnBackgroundGrid();
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }
            UpdateGridRootVisibility();
            RefreshDraggableInteractable();
        }

        private void OnEnable()
        {
            UpdateGridRootVisibility();
        }

        private void OnDisable()
        {
            if (activeGridVisuals != null)
            {
                foreach (GameObject visual in activeGridVisuals)
                {
                    if (visual != null)
                    {
                        visual.SetActive(false);
                    }
                }
            }
        }

        public void SpinAllGearsOnceVisual()
        {
            foreach (GearView view in viewsByNode.Values)
            {
                if (view != null)
                {
                    view.SpinOnceVisual();
                }
            }
        }

        public void SetAllGearsRapidSpin(bool enabled)
        {
            isRapidSpinningAll = enabled;
            foreach (GearView view in viewsByNode.Values)
            {
                if (view != null)
                {
                    view.SetRapidSpin(enabled);
                }
            }
        }

        protected override void OnUnbind()
        {
            if (viewModel != null)
            {
                viewModel.OnGearPlaced -= HandleGearPlaced;
                viewModel.OnGearRemoved -= HandleGearRemoved;
                viewModel.OnGearTriggered -= HandleGearTriggered;
                viewModel.OnGearChargeCompleted -= HandleGearChargeCompleted;
            }

            if (animator != null)
            {
                animator.Clear();
            }

            DestroyAllViews();

            base.OnUnbind();
        }

        private void RefreshDraggableInteractable()
        {
            bool boardInteractable = workspaceInteractionEnabled && viewModel != null && viewModel.Interactable;
            foreach (KeyValuePair<IGridNode, GearView> pair in viewsByNode)
            {
                Draggable drag = pair.Value != null ? pair.Value.GetComponent<Draggable>() : null;
                if (drag == null)
                {
                    continue;
                }

                IGridNode node = pair.Key;
                bool movable = node != null && node.IsInteractable && node.ConfigData != null && node.ConfigData.IsMovable;
                drag.IsInteractable = boardInteractable && movable;
            }
        }

        internal Vector2Int BoardLocalToGrid(Vector2 boardLocal)
        {
            if (boardLayout == null || viewModel == null)
            {
                return Vector2Int.zero;
            }

            return boardLayout.GetGridPosition(boardLocal, viewModel.BoardRules);
        }

        private void HandleGearPlaced(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            SpawnView(node);
        }

        private void Start()
        {
            cachedCarView = UnityEngine.Object.FindObjectOfType<global::GearEngine.CarSimulation.Presentation.CarView>();
        }

        private void HandleGearTriggered(Vector2Int pos, string text, float duration)
        {
            Transform targetTransform = ResolveFeedbackTarget(pos, out bool isCarTarget);
            if (targetTransform == null)
            {
                return;
            }
            Vector3? moveDir = ResolveFeedbackDirection(targetTransform, isCarTarget);
            Transform carRef = isCarTarget ? targetTransform : null;
            int parsedScore = ParseScore(text);
            System.Action onExplode = () => PublishCombatTextScore(parsedScore);
            SpawnFloatingText(targetTransform, text, duration, moveDir, carRef, onExplode, isCarTarget);
        }

        private Transform ResolveFeedbackTarget(Vector2Int pos, out bool isCarTarget)
        {
            cachedCarView ??= UnityEngine.Object.FindObjectOfType<global::GearEngine.CarSimulation.Presentation.CarView>();
            isCarTarget = cachedCarView != null;
            if (!isCarTarget)
            {
                return GetSlotTransform(pos);
            }
            Transform innerCar = cachedCarView.transform.Find("Car");
            return innerCar != null ? innerCar : cachedCarView.transform;
        }

        private Vector3? ResolveFeedbackDirection(Transform targetTransform, bool isCarTarget)
        {
            if (!isCarTarget || cachedCarView == null)
            {
                return null;
            }
            float speedMultiplier = Mathf.Max(1f, Mathf.Abs(cachedCarView.CurrentSpeed) / 125f);
            return -targetTransform.forward * speedMultiplier;
        }

        private int ParseScore(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 50;
            }
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
            return match.Success && int.TryParse(match.Value, out int score) ? score : 50;
        }

        private void PublishCombatTextScore(int score)
        {
            viewModel?.PublishCombatTextExploded(score);
        }

        private void SpawnFloatingText(Transform target, string text, float duration, Vector3? direction, Transform carRef, System.Action onExplode, bool isCarTarget)
        {
            if (floatingTextPrefab != null)
            {
                FloatingText instance = Instantiate(floatingTextPrefab, target.position, Quaternion.identity);
                if (isCarTarget)
                {
                    instance.SetBaseScale(0.3f);
                }
                instance.Play(text, duration, direction, carRef, onExplode);
                return;
            }
            FloatingText.Spawn(target.position, text, duration, direction, carRef, onExplode);
        }

        private void HandleGearChargeCompleted(IGridNode node)
        {
            if (node == null || !viewsByNode.TryGetValue(node, out GearView view))
            {
                return;
            }

            view.PlayChargeCompleteFeedback();

            if (node.ConfigData != null && node.ConfigData.ChargeCompleteSound.IsValid())
            {
                Ami.BroAudio.BroAudio.Play(node.ConfigData.ChargeCompleteSound);
            }
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
            animator.Untrack(node);
            viewsByNode.Remove(node);
            DestroyViewGameObject(view.gameObject);

            UpdateGridRootVisibility();
        }

        private void SpawnView(IGridNode node)
        {
            if (node == null || viewsByNode.ContainsKey(node))
            {
                return;
            }
            if (!TryResolveSpawnSlot(node, out Transform slot))
            {
                return;
            }
            GearView view = GearViewSpawner.Spawn(node.ConfigData, slot);
            if (view == null)
            {
                return;
            }
            RegisterSpawnedView(node, view);
        }

        private void RegisterSpawnedView(IGridNode node, GearView view)
        {
            view.OnClicked += () => HandleViewClicked(node);
            viewsByNode[node] = view;
            animator.Track(node, view);
            WireBoardDraggable(view, node);
            if (isRapidSpinningAll)
            {
                view.SetRapidSpin(true);
            }
            UpdateGridRootVisibility();
        }

        private bool TryResolveSpawnSlot(IGridNode node, out Transform slot)
        {
            slot = GetSlotTransform(node.Position);
            if (slot != null && node.ConfigData?.ViewPrefab != null)
            {
                return true;
            }
            Debug.LogError($"[BoardView] Cannot spawn gear: slot or ViewPrefab missing for '{node.ConfigData?.Id}' at {node.Position}.");
            return false;
        }

        private void HandleViewClicked(IGridNode node)
        {
            Debug.Log($"[BoardViewComponent] view.OnClicked fired for node '{node.ConfigData?.Id}'");
            viewModel?.HandleBoardClick(node);
        }

        private void WireBoardDraggable(GearView view, IGridNode node)
        {
            Draggable drag = view.GetComponent<Draggable>();
            if (drag == null)
            {
                Debug.LogError($"[BoardView] Gear '{node.ConfigData?.Id}' ViewPrefab must include Draggable on the root.");
                return;
            }
            if (!ValidateBoardDragContext(drag))
            {
                return;
            }
            ConfigureBoardDrag(drag, node);
        }

        private void ConfigureBoardDrag(Draggable drag, IGridNode node)
        {
            drag.SetHideSourceWhileDragging(true);
            drag.Configure(dragService, dragOverlay);
            bool boardInteractable = workspaceInteractionEnabled && viewModel != null && viewModel.Interactable;
            bool movable = node.IsInteractable && node.ConfigData != null && node.ConfigData.IsMovable;
            drag.IsInteractable = boardInteractable && movable;
            drag.BuildPayload = e => new DragPayload(node, e.position);
            drag.OnDropAccepted = target => OnGearDragAccepted(node, target);
        }

        private bool ValidateBoardDragContext(Draggable drag)
        {
            if (!workspaceInteractionEnabled)
            {
                drag.IsInteractable = false;
                return false;
            }
            if (dragService != null && dragOverlay != null)
            {
                return true;
            }
            drag.IsInteractable = false;
            Debug.LogError("[BoardView] Drag context is missing.");
            return false;
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

        private void DestroyAllViews()
        {
            DestroyGearViews();
            viewsByNode.Clear();
            slotByCoord.Clear();
            DestroyBackgroundSlots();
            backgroundSlots.Clear();
        }

        private void DestroyGearViews()
        {
            foreach (KeyValuePair<IGridNode, GearView> pair in viewsByNode)
            {
                if (pair.Value != null)
                {
                    DestroyViewGameObject(pair.Value.gameObject);
                }
            }
        }

        private void DestroyBackgroundSlots()
        {
            foreach (GameObject slot in backgroundSlots)
            {
                if (slot != null)
                {
                    DestroyViewGameObject(slot);
                }
            }
        }

        private void SpawnBackgroundGrid()
        {
            BoardRulesSO rules = viewModel.BoardRules;
            Assert.IsNotNull(rules, "[BoardView] BoardRulesSO is missing.");

            for (int x = 0; x < rules.GridWidth; x++)
            {
                for (int y = 0; y < rules.GridHeight; y++)
                {
                    SpawnBackgroundSlot(new Vector2Int(x, y), rules);
                }
            }
        }

        private void SpawnBackgroundSlot(Vector2Int pos, BoardRulesSO rules)
        {
            GameObject slotView = Instantiate(gridSlotPrefab, gridRoot, false);
            RectTransform slotRect = slotView.transform as RectTransform;
            if (slotRect == null)
            {
                Debug.LogError("[BoardView] Grid slot prefab must use RectTransform.");
                DestroyViewGameObject(slotView);
                return;
            }
            slotRect.anchoredPosition = boardLayout.GetCellLocalPosition(pos, rules);
            slotView.name = $"GridSlot_{pos.x}_{pos.y}";
            slotByCoord[pos] = slotRect;
            backgroundSlots.Add(slotView);
        }

        private void DestroyViewGameObject(GameObject go)
        {
            go.SafeDestroy();
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<GearItemData>() != null || payload.GetData<IGridNode>() != null;
        }

        public bool OnDrop(DragPayload payload)
        {
            if (viewModel == null || boardLayout == null)
            {
                return false;
            }
            RectTransform boardRoot = GetBoardSpaceRoot();
            Canvas canvas = boardRoot != null ? boardRoot.GetComponentInParent<Canvas>() : null;
            if (!TryResolveDropGridPosition(payload.ScreenPosition, boardRoot, canvas, out Vector2Int gridPos))
            {
                return false;
            }
            return ApplyDropPayload(payload, gridPos);
        }

        private bool TryResolveDropGridPosition(Vector2 screenPosition, RectTransform boardRoot, Canvas canvas, out Vector2Int gridPos)
        {
            gridPos = Vector2Int.zero;
            if (!BoardScreenPositionUtility.TryGetLocalPoint(boardRoot, canvas, screenPosition, out Vector2 boardLocal))
            {
                return false;
            }
            return boardLayout.TryGetGridPosition(boardLocal, viewModel.BoardRules, out gridPos);
        }

        private bool ApplyDropPayload(DragPayload payload, Vector2Int gridPos)
        {
            IGridNode draggedNode = payload.GetData<IGridNode>();
            if (draggedNode != null)
            {
                return viewModel.TryMoveBoardGear(draggedNode, gridPos);
            }
            GearItemData gear = payload.GetData<GearItemData>();
            if (gear != null)
            {
                return viewModel.HandleInventoryDrop(gridPos, gear);
            }
            return false;
        }

        public RectTransform GetBoardSpaceRoot()
        {
            if (gearsRoot != null)
            {
                return gearsRoot;
            }
            if (gridRoot != null)
            {
                return gridRoot;
            }
            return transform as RectTransform;
        }

        private Transform GetSlotTransform(Vector2Int pos)
        {
            return slotByCoord.TryGetValue(pos, out Transform slot) ? slot : null;
        }

        private void UpdateGridRootVisibility()
        {
            bool hasGears = isActiveAndEnabled && viewsByNode.Count > 0;
            if (activeGridVisuals == null)
            {
                return;
            }
            foreach (GameObject visual in activeGridVisuals)
            {
                if (visual != null)
                {
                    visual.SetActive(hasGears);
                }
            }
        }
    }
}
