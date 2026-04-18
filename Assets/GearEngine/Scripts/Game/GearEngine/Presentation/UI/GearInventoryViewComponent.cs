using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryViewComponent : ViewComponent<GearInventoryViewModel>, IDragTarget
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        private Transform boardScaleReference;
        private BoardViewComponent boardView;

        private bool inventoryUiBinding;

        /// <summary>
        /// Wires the board root used to match inventory gear scale to the board (and resolves <see cref="BoardViewComponent"/> for drag scaling).
        /// </summary>
        public void SetBoardScaleReference(Transform reference)
        {
            boardScaleReference = reference;
            boardView = reference != null ? reference.GetComponentInParent<BoardViewComponent>() : null;
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
                CheckTargetRect();
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

        private void CheckTargetRect()
        {
            if (boardScaleReference == null)
            {
                Debug.LogWarning("[GearInventoryView] Board scale reference is not wired from GearEngineCoreViewComponent; gear ghost scaling may be incorrect.");
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

            // Destroy() is deferred until end of frame; multiple RebuildUIList calls in one frame
            // (e.g. OnBind + InventoryListRevision binding) would stack new slots on top of queued destroys.
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
            dragger.GhostScaleMultiplier = totalScale;
            GameObject visualObj = GearVisualSetup.SetupVisual(visualContainer, gear, totalScale);
            if (visualObj != null)
            {
                dragger.GhostPrefab = visualObj;
            }

            slotView.Bind(gear, viewModel);
            HookDragHandlers(dragger, gear, visualContainer);
        }

        private float ComputeBaseScale(Transform visualContainer)
        {
            float baseScale = 56f;
            if (boardScaleReference != null && visualContainer.lossyScale.x > 0f)
            {
                baseScale = boardScaleReference.lossyScale.x / visualContainer.lossyScale.x;
            }

            return baseScale;
        }

        private void HookDragHandlers(DragHandler dragger, GearConfigData gear, Transform visualContainer)
        {
            GearConfigData capturedGear = gear;
            dragger.OnDragBegin += () => viewModel.NotifySlotDragStarted(capturedGear);
            dragger.OnDragEnd += () => viewModel.NotifySlotDragEnded();
            dragger.BuildPayload = worldPos => new DragPayload(capturedGear, worldPos, dragger);
            dragger.OnDragAccepted = _ => viewModel.NotifySlotDragAccepted(capturedGear);
            dragger.GhostUniformScaleResolver = screenPos => ResolveGhostUniformScale(capturedGear, visualContainer, dragger, screenPos);
        }

        private float ResolveGhostUniformScale(GearConfigData gear, Transform visualContainer, DragHandler dragger, Vector2 screenPos)
        {
            float slotRelativeScale = gear.RelativeScaleMultiplier * ComputeBaseScale(visualContainer);
            Camera cam = Camera.main;
            if (cam == null || boardView == null)
            {
                return slotRelativeScale;
            }

            var payload = new DragPayload(gear, Vector3.zero, dragger);
            IDragTarget target = DragTargetFinder.Find(payload, screenPos, cam);
            if (target is BoardViewComponent board && ReferenceEquals(board, boardView))
            {
                Transform root = board.GetBoardSpaceRoot();
                if (root != null && visualContainer != null && visualContainer.lossyScale.x > 0f)
                {
                    return gear.RelativeScaleMultiplier * (root.lossyScale.x / visualContainer.lossyScale.x);
                }
            }

            return slotRelativeScale;
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
