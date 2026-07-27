using System.Linq;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using DG.Tweening;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryViewComponent : ViewComponent<GearInventoryViewModel>, IDragTarget
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        private bool inventoryUiBinding;
        private IDragService dragService;
        private RectTransform dragOverlay;

        public void SetDragContext(IDragService service, RectTransform overlay)
        {
            dragService = service;
            dragOverlay = overlay;
        }

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[GearInventoryView] ViewModel is missing.");
            Assert.IsNotNull(viewModel.TrayItems, "[GearInventoryView] Tray items collection is missing.");

            inventoryUiBinding = true;
            try
            {
                InitializeBindings();
            }
            finally
            {
                inventoryUiBinding = false;
            }
        }

        private void InitializeBindings()
        {
            if (inventoryLimitLabel != null)
            {
                inventoryLimitLabel.text = string.Empty;
            }
            Bind<int, int>(() => viewModel.InventoryListRevision, OnInventoryListRevisionChanged);
            Bind<GearItemData, GearItemData>(() => viewModel.SelectedItem, OnSelectionChanged);
            Bind<int, int>(() => viewModel.CurrentBoardGears, _ => UpdateCapacityLabel());
            Bind<int, int>(() => viewModel.MaxBoardGears, _ => UpdateCapacityLabel());
            UpdateCapacityLabel();
        }

        private void UpdateCapacityLabel()
        {
            if (inventoryLimitLabel != null)
            {
                inventoryLimitLabel.text = $"{viewModel.CurrentBoardGears}/{viewModel.MaxBoardGears}";
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

        private void OnSelectionChanged(GearItemData newItem)
        {
            if (newItem != null)
            {
                Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay moved onto -> {newItem.Id}");
            }
            else
            {
                Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Highlight overlay disabled (None selected).");
            }
        }

        public void RebuildAndFit()
        {
            RebuildUIList();
        }

        private void RebuildUIList()
        {
            ClearInventorySlots();
            foreach (GearItemData item in viewModel.TrayItems)
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
                DestroySlot(itemsContainer.GetChild(i).gameObject);
            }
        }

        private void DestroySlot(GameObject slot)
        {
            if (Application.isPlaying)
            {
                Destroy(slot);
                return;
            }
            DestroyImmediate(slot);
        }

        private void AddPresenterForItem(GearItemData item)
        {
            GameObject slotObj = CreateSlotObject(item);
            if (slotObj == null)
            {
                return;
            }

            WireGearSlot(slotObj, item);
        }

        private void WireGearSlot(GameObject slotObj, GearItemData gear)
        {
            GearInventorySlotView slotView = slotObj.GetComponent<GearInventorySlotView>();
            Draggable drag = slotObj.GetComponent<Draggable>();
            if (!ValidateSlotParts(slotView, drag))
            {
                return;
            }
            ConfigureSlotDrag(drag, gear);
            ApplyGearVisualAndDrag(slotView, gear);
        }

        private void ConfigureSlotDrag(Draggable drag, GearItemData gear)
        {
            drag.SetHideSourceWhileDragging(false);
            drag.Configure(dragService, dragOverlay);
            GearItemData capturedGear = gear;
            drag.BuildPayload = e => BuildSlotPayload(capturedGear, e.position);
            drag.OnDropAccepted = _ => viewModel.NotifySlotDragAccepted(capturedGear);
            drag.OnDropRejected = HandleSlotDropRejected;
        }

        private DragPayload BuildSlotPayload(GearItemData gear, Vector2 screenPosition)
        {
            HandleSlotDropRejected();
            return new DragPayload(gear, screenPosition);
        }

        private void HandleSlotDropRejected()
        {
            if (viewModel.CurrentBoardGears >= viewModel.MaxBoardGears)
            {
                PunchCapacityLabel();
            }
        }

        private void ApplyGearVisualAndDrag(GearInventorySlotView slotView, GearItemData gear)
        {
            if (gear.ViewPrefab == null)
            {
                Debug.LogError($"[GearInventoryView] Gear '{gear?.Id}' has no ViewPrefab; inventory visual skipped.");
                slotView.Bind(gear, viewModel);
                return;
            }

            GearView view = GearViewSpawner.Spawn(gear, slotView.VisualContainer);
            if (view == null)
            {
                slotView.Bind(gear, viewModel);
                return;
            }
            ConfigureInventoryGearView(view);
            slotView.Bind(gear, viewModel);
        }

        private void ConfigureInventoryGearView(GearView view)
        {
            view.SetChargeFillTarget(1f, snap: true);
            view.SettleNow();
            DisableInventoryVisualInteraction(view);
        }

        private void DisableInventoryVisualInteraction(GearView view)
        {
            Draggable draggable = view.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.enabled = false;
            }

            foreach (Graphic graphic in view.GetComponentsInChildren<Graphic>(includeInactive: true))
            {
                graphic.raycastTarget = false;
            }
        }

        public bool OnDrop(DragPayload payload)
        {
            return CanAccept(payload);
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<IGridNode>()?.ConfigData?.IsReturnable == true;
        }

        private bool ValidateSlotParts(GearInventorySlotView slotView, Draggable drag)
        {
            if (slotView == null)
            {
                Debug.LogError("[GearInventoryView] Slot prefab must include GearInventorySlotView (see GearSlot prefab).");
                return false;
            }
            if (drag == null)
            {
                Debug.LogError("[GearInventoryView] Slot prefab must include Draggable (see GearSlot prefab).");
                return false;
            }
            return ValidateDragContext();
        }

        private bool ValidateDragContext()
        {
            if (dragService != null && dragOverlay != null)
            {
                return true;
            }
            Debug.LogError("[GearInventoryView] Drag context is missing.");
            return false;
        }

        private void PunchCapacityLabel()
        {
            if (inventoryLimitLabel == null)
            {
                return;
            }
            inventoryLimitLabel.transform.DOKill(complete: true);
            inventoryLimitLabel.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.3f, 10, 1f);
        }

        private GameObject CreateSlotObject(GearItemData item)
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
            if (viewModel.TrayItems.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.TrayItems.First());
            }
        }
    }
}
