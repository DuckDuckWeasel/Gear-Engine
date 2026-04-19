using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryViewComponent : ViewComponent<GearInventoryViewModel>, IDragTarget
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;

        private bool inventoryUiBinding;

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

        /// <summary>Rebuilds inventory slot UI and fits gear visuals to each slot rect. Call after the owning view hierarchy is active and layout is final (e.g. after FrustumFit open transition).</summary>
        public void RebuildAndFit()
        {
            RebuildUIList();
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
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
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
            GearInventorySlotView slotView = slotObj.GetComponent<GearInventorySlotView>();
            if (slotView == null)
            {
                Debug.LogError("[GearInventoryView] Slot prefab must include GearInventorySlotView (see GearSlot prefab).");
                return;
            }

            Draggable drag = slotObj.GetComponent<Draggable>() ?? slotObj.AddComponent<Draggable>();
            drag.SetHideSourceWhileDragging(false);
            GearConfigData capturedGear = gear;
            drag.BuildPayload = e =>
            {
                Vector3 world = DragPointerUtility.GetWorldPosition(e);
                return new DragPayload(capturedGear, world);
            };

            drag.OnDropAccepted = _ => viewModel.NotifySlotDragAccepted(capturedGear);

            ApplyGearVisualAndDrag(slotView, gear, drag);
        }

        private void ApplyGearVisualAndDrag(GearInventorySlotView slotView, GearConfigData gear, Draggable drag)
        {
            const int inventorySortingBase = 50;

            if (gear.ViewPrefab == null)
            {
                Debug.LogError($"[GearInventoryView] Gear '{gear?.Id}' has no ViewPrefab; inventory visual skipped.");
                slotView.Bind(gear, viewModel);
                return;
            }

            Transform parent = slotView.VisualContainer;
            float scaleMultiplier = gear.RelativeScaleMultiplier;
            GearView view = Instantiate(gear.ViewPrefab, parent, false);
            view.BindForDisplay(gear, DisplayOptions.Inventory(inventorySortingBase, scaleMultiplier));

            // Gear visuals live in world space (sized for board cells); make them fill the
            // inventory slot's rect by rescaling after layout has had a chance to settle.
            RectTransform slotRect = parent as RectTransform;
            if (slotRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(slotRect);
                Canvas.ForceUpdateCanvases();
                view.FitVisualToRect(slotRect);
            }

            slotView.Bind(gear, viewModel);
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<IGridNode>()?.ConfigData?.IsReturnable == true;
        }

        public bool OnDrop(DragPayload payload)
        {
            return CanAccept(payload);
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
    }
}
