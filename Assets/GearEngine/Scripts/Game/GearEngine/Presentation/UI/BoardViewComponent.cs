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
using Ami.BroAudio;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardViewComponent : ViewComponent<BoardViewModel>, IDragTarget
    {
        [SerializeField] private GameObject gridSlotPrefab;
        [SerializeField] private Transform gridRoot;
        [Tooltip("Legacy root for board-space plane. Gears parent to grid slots.")]
        [SerializeField] private Transform gearsRoot;
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

        public BoardLayoutSO BoardLayout => boardLayout;

        public new void Unbind()
        {
            base.Unbind();
        }

        protected override void OnBind()
        {
            Assert.IsNotNull(boardLayout, "[BoardView] BoardLayoutSO is missing.");
            Assert.IsNotNull(animator, "[BoardView] BoardGearAnimator is missing.");

            // Board gears are world-space colliders; the EventSystem only dispatches
            // IBeginDragHandler to them via PhysicsRaycaster (3D) / Physics2DRaycaster
            // on the rendering camera. We add both so future gear prefabs can use either.
            EnsureBoardCameraRaycasters();

            animator.Configure(GetSlotTransform, boardLayout, viewModel.MotorCogGearId, 
                () => viewModel.IsSimulationRunning || (viewModel.EngineService != null && viewModel.EngineService.IsRunning));

            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;
            viewModel.OnGearTriggered += HandleGearTriggered;
            viewModel.OnGearChargeCompleted += HandleGearChargeCompleted;

            SpawnBackgroundGrid();
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }
            
            UpdateGridRootVisibility();

            viewModel.PropertyChanged += OnBoardViewModelPropertyChanged;
            RefreshDraggableInteractable();
            Bind(() => viewModel.BoardLimitText, () => boardLimitLabel.text);
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

        private void UpdateGridRootVisibility()
        {
            // Only show visuals if the component is enabled AND there are gears.
            bool hasGears = isActiveAndEnabled && viewsByNode.Count > 0;
            if (activeGridVisuals != null)
            {
                foreach (GameObject visual in activeGridVisuals)
                {
                    if (visual != null)
                    {
                        visual.SetActive(hasGears);
                    }
                }
            }
        }

        public void SpinAllGearsOnceVisual()
        {
            foreach (GearView view in viewsByNode.Values)
            {
                if (view != null) view.SpinOnceVisual();
            }
        }

        public void SetAllGearsRapidSpin(bool enabled)
        {
            isRapidSpinningAll = enabled;
            foreach (GearView view in viewsByNode.Values)
            {
                if (view != null) view.SetRapidSpin(enabled);
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
                viewModel.PropertyChanged -= OnBoardViewModelPropertyChanged;
            }

            if (animator != null)
            {
                animator.Clear();
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

        private global::GearEngine.CarSimulation.Presentation.CarView cachedCarView;

        private void Start()
        {
            cachedCarView = UnityEngine.Object.FindObjectOfType<global::GearEngine.CarSimulation.Presentation.CarView>();
        }

        private void HandleGearTriggered(Vector2Int pos, string text, float duration)
        {
            Transform targetTransform = null;
            bool isCarTarget = false;
            
            if (cachedCarView == null)
            {
                cachedCarView = UnityEngine.Object.FindObjectOfType<global::GearEngine.CarSimulation.Presentation.CarView>();
            }

            if (cachedCarView != null)
            {
                Transform innerCar = cachedCarView.transform.Find("Car");
                targetTransform = innerCar != null ? innerCar : cachedCarView.transform;
                isCarTarget = true;
            }
            else
            {
                targetTransform = GetSlotTransform(pos);
            }
            
            if (targetTransform == null) return;
            
            // Move in the opposite direction of the car's forward vector
            Vector3? moveDir = isCarTarget ? -targetTransform.forward : (Vector3?)null;
            Transform carRef = isCarTarget ? targetTransform : null;

            if (isCarTarget && moveDir.HasValue && cachedCarView != null)
            {
                // Multiply the direction vector by a factor of the car's speed.
                float speedMultiplier = Mathf.Max(1f, Mathf.Abs(cachedCarView.CurrentSpeed) / 125f);
                moveDir = moveDir.Value * speedMultiplier;
            }
            
            int parsedScore = 50; // default points
            if (!string.IsNullOrEmpty(text))
            {
                var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int s))
                {
                    parsedScore = s;
                }
            }

            System.Action onExplode = () => {
                if (viewModel != null)
                {
                    viewModel.PublishCombatTextExploded(parsedScore);
                }
            };
            
            if (floatingTextPrefab != null)
            {
                FloatingText instance = Instantiate(floatingTextPrefab, targetTransform.position, Quaternion.identity);
                if (isCarTarget)
                {
                    instance.SetBaseScale(0.3f);
                }
                instance.Play(text, duration, moveDir, carRef, onExplode);
            }
            else
            {
                FloatingText.Spawn(targetTransform.position, text, duration, moveDir, carRef, onExplode);
            }
        }

        private void HandleGearChargeCompleted(IGridNode node)
        {
            if (node == null || !viewsByNode.TryGetValue(node, out GearView view)) return;
            
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
            if (node == null)
            {
                return;
            }

            if (viewsByNode.TryGetValue(node, out GearView existingView))
            {
                return;
            }

            Transform slot = GetSlotTransform(node.Position);
            if (slot == null || node.ConfigData?.ViewPrefab == null)
            {
                Debug.LogError($"[BoardView] Cannot spawn gear: slot or ViewPrefab missing for '{node.ConfigData?.Id}' at {node.Position}.");
                return;
            }

            GearView view = GearViewSpawner.Spawn(node.ConfigData, slot);
            if (view == null)
            {
                return;
            }

            view.OnClicked += () => 
            { 
                Debug.Log($"[BoardViewComponent] view.OnClicked fired for node '{node.ConfigData?.Id}'");
                if (viewModel != null) 
                    viewModel.HandleBoardClick(node); 
            };

            viewsByNode[node] = view;
            animator.Track(node, view);
            WireBoardDraggable(view, node);

            if (isRapidSpinningAll)
            {
                view.SetRapidSpin(true);
            }
            
            UpdateGridRootVisibility();
        }

        private void WireBoardDraggable(GearView view, IGridNode node)
        {
            GameObject go = view.gameObject;
            Draggable drag = go.GetComponent<Draggable>();
            bool hasCollider = go.GetComponent<Collider2D>() != null || go.GetComponent<Collider>() != null;
            if (!hasCollider || drag == null)
            {
                Debug.LogError($"[BoardView] Gear '{node.ConfigData?.Id}' ViewPrefab must include Draggable and a Collider (2D or 3D) on the root.");
                return;
            }

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

        private static void EnsureBoardCameraRaycasters()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[BoardView] Camera.main is null; board gears will not receive drag events.");
                return;
            }

            EnsureCameraComponent(cam, "UnityEngine.EventSystems.PhysicsRaycaster, UnityEngine.UI");
            EnsureCameraComponent(cam, "UnityEngine.EventSystems.Physics2DRaycaster, UnityEngine.UI");
        }

        private static void EnsureCameraComponent(Camera cam, string assemblyQualifiedName)
        {
            Type raycasterType = Type.GetType(assemblyQualifiedName);
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
            return payload.GetData<GearItemData>() != null || payload.GetData<IGridNode>() != null;
        }

        public bool OnDrop(DragPayload payload)
        {
            GearItemData gear = payload.GetData<GearItemData>();
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
