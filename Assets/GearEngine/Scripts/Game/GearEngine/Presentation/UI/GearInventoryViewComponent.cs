using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryViewComponent : ViewComponent<GearInventoryViewModel>, IDragTarget
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        private Transform boardRoot;
        private DragGhostController ghostController;

        private bool inventoryUiBinding;

        /// <summary>
        /// Wires the board root used for inventory slot scale matching and for parenting the drag ghost.
        /// </summary>
        public void SetBoardRoot(Transform root)
        {
            boardRoot = root;
            ghostController = root != null ? new DragGhostController(root) : null;
        }

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[GearInventoryView] ViewModel is missing.");
            Assert.IsNotNull(viewModel.InventoryModel?.Items, "[GearInventoryView] Inventory items collection is missing.");

            inventoryUiBinding = true;
            try
            {
                Bind(() => viewModel.InventoryLimitText, () => inventoryLimitLabel.text);
                Bind<int, int>(() => viewModel.InventoryListRevision, OnInventoryListRevisionChanged);
                Bind<IItem, IItem>(() => viewModel.SelectedItem, OnSelectionChanged);
                CheckBoardRoot();
                RebuildUIList();
            }
            finally
            {
                inventoryUiBinding = false;
            }
        }

        private void OnInventoryListRevisionChanged(int _)
        {
            if (inventoryUiBinding)
            {
                return;
            }

            RebuildUIList();
        }

        private void CheckBoardRoot()
        {
            if (boardRoot == null)
            {
                Debug.LogWarning("[GearInventoryView] Board root is not wired from GearEngineCoreViewComponent; slot scaling and drag ghost may be incorrect.");
            }
        }

        private void OnSelectionChanged(IItem newItem)
        {
            if (newItem is GearConfigData newSelection)
            {
                if (newSelection != null)
                {
                    Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay moved onto -> {newSelection.Id}");
                }
                else
                {
                    Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay disabled (None selected).");
                }
            }
        }

        private void RebuildUIList()
        {
            ClearInventorySlots();
            foreach (IItem item in viewModel.InventoryModel.Items)
            {
                AddPresenterForItem(item);
            }
        }

        private void ClearInventorySlots()
        {
            if (itemsContainer == null)
            {
                return;
            }

            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = itemsContainer.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        private void AddPresenterForItem(IItem item)
        {
            GameObject slotObj = CreateSlotObject(item);
            if (slotObj == null)
            {
                return;
            }

            if (item is GearConfigData gear)
            {
                WireGearSlot(slotObj, gear);
            }
        }

        private void WireGearSlot(GameObject slotObj, GearConfigData gear)
        {
            DragHandler dragger = CreateDragHandler(slotObj);
            GearInventorySlotView slotView = CreateGearInventorySlotView(slotObj);
            Transform visualContainer = BuildVisualContainer(slotObj);
            ApplyGearVisualAndDrag(visualContainer, gear, dragger, slotView);
        }

        private void ApplyGearVisualAndDrag(Transform visualContainer, GearConfigData gear, DragHandler dragger, GearInventorySlotView slotView)
        {
            float totalScale = gear.RelativeScaleMultiplier * ComputeBaseScale(visualContainer);
            GameObject visualObj = GearVisualSetup.SetupVisual(visualContainer, gear, totalScale);
            if (visualObj == null)
            {
                Debug.LogError($"[GearInventoryView] SetupVisual returned null for gear '{gear?.Id}'.");
            }

            slotView.Bind(gear, viewModel);
            HookDragHandlers(dragger, gear);
        }

        private float ComputeBaseScale(Transform visualContainer)
        {
            float baseScale = 56f;
            if (boardRoot != null && visualContainer.lossyScale.x > 0f)
            {
                baseScale = boardRoot.lossyScale.x / visualContainer.lossyScale.x;
            }

            return baseScale;
        }

        private void HookDragHandlers(DragHandler dragger, GearConfigData gear)
        {
            GearConfigData capturedGear = gear;
            dragger.OnDragBegin += BeginInventoryDrag;
            dragger.OnDragMoved += MoveInventoryDragGhost;
            dragger.OnDragEnd += EndInventoryDrag;
            dragger.BuildPayload = worldPos => new DragPayload(capturedGear, worldPos, dragger);
            dragger.OnDragAccepted = _ => viewModel.NotifySlotDragAccepted(capturedGear);

            void BeginInventoryDrag(PointerEventData e)
            {
                viewModel.NotifySlotDragStarted(capturedGear);
                if (ghostController == null)
                {
                    return;
                }

                ghostController.CreateGhost(capturedGear);
                if (DragHandler.TryGetPointerWorldPosition(e, out Vector3 w))
                {
                    ghostController.MoveGhostTo(w);
                }
            }

            void MoveInventoryDragGhost(PointerEventData e)
            {
                if (ghostController == null)
                {
                    return;
                }

                if (DragHandler.TryGetPointerWorldPosition(e, out Vector3 w))
                {
                    ghostController.MoveGhostTo(w);
                }
            }

            void EndInventoryDrag(PointerEventData e)
            {
                ghostController?.DestroyGhost();
                viewModel.NotifySlotDragEnded();
            }
        }

        public void OnDragStarted(DragPayload payload)
        {
        }

        public void OnDragEnded()
        {
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<IGridNode>()?.ConfigData?.IsReturnable == true;
        }

        public void OnDrop(DragPayload payload)
        {
            payload.Source?.OnDropAccepted(this);
        }

        public void OnHoverEnter(DragPayload payload)
        {
        }

        public void OnHoverExit()
        {
        }

        private GameObject CreateSlotObject(IItem item)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("[GearInventoryView] Slot Prefab is missing.");
                return null;
            }

            GameObject slot = Instantiate(slotPrefab, itemsContainer);
            slot.name = $"Slot_{item.Id}";
            return slot;
        }

        [ContextMenu("Mock: UI Click First Available Gear")]
        public void MockClickFirstGear()
        {
            if (viewModel.InventoryModel.Items.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.InventoryModel.Items.First() as GearConfigData);
            }
        }

        private static DragHandler CreateDragHandler(GameObject slotObj)
        {
            DragHandler dragger = slotObj.GetComponent<DragHandler>();
            if (dragger == null)
            {
                dragger = slotObj.AddComponent<DragHandler>();
            }

            return dragger;
        }

        private static GearInventorySlotView CreateGearInventorySlotView(GameObject slotObj)
        {
            GearInventorySlotView slotView = slotObj.GetComponent<GearInventorySlotView>();
            if (slotView == null)
            {
                slotView = slotObj.AddComponent<GearInventorySlotView>();
            }

            return slotView;
        }

        private static Transform BuildVisualContainer(GameObject slotObj)
        {
            Transform visualContainer = slotObj.transform.Find("VisualContainer");
            return visualContainer != null ? visualContainer : slotObj.transform;
        }
    }
}
