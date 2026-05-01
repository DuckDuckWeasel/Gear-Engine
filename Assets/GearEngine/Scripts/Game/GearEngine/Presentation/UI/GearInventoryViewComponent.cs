using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
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

        private bool inventoryUiBinding;

        protected override void OnBind()
        {
            Assert.IsNotNull(viewModel, "[GearInventoryView] ViewModel is missing.");
            Assert.IsNotNull(viewModel.TrayItems, "[GearInventoryView] Tray items collection is missing.");

            inventoryUiBinding = true;
            try
            {
                if (inventoryLimitLabel != null)
                {
                    inventoryLimitLabel.text = string.Empty;
                }

                Bind<int, int>(() => viewModel.InventoryListRevision, OnInventoryListRevisionChanged);
                Bind<GearItemData, GearItemData>(() => viewModel.SelectedItem, OnSelectionChanged);
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

        /// <summary>Rebuilds inventory slot UI and fits gear visuals to each slot rect. Call after the owning view hierarchy is active and layout is final (e.g. after FrustumFit open transition).</summary>
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
            if (slotView == null)
            {
                Debug.LogError("[GearInventoryView] Slot prefab must include GearInventorySlotView (see GearSlot prefab).");
                return;
            }

            Draggable drag = slotObj.GetComponent<Draggable>();
            if (drag == null)
            {
                Debug.LogError("[GearInventoryView] Slot prefab must include Draggable (see GearSlot prefab).");
                return;
            }

            drag.SetHideSourceWhileDragging(false);
            GearItemData capturedGear = gear;
            drag.BuildPayload = e =>
            {
                Vector3 world = DragPointerUtility.GetWorldPosition(e);
                return new DragPayload(capturedGear, world);
            };

            drag.OnDropAccepted = _ => viewModel.NotifySlotDragAccepted(capturedGear);

            ApplyGearVisualAndDrag(slotView, gear);
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

            view.SetChargeFillTarget(0f, snap: true);
            view.SettleNow();

            // Gear prefabs ship with physics colliders for board-side raycasting
            // (PhysicsRaycaster / Physics2DRaycaster). Inside an inventory slot the gear
            // visual sits in front of the slot's UI Image, and those colliders steal the
            // pointer from the GraphicRaycaster - so OnBeginDrag never reaches the slot's
            // Draggable. The slot is the drag source here, not the gear visual.
            DisableInteractionColliders(view.gameObject);

            slotView.Bind(gear, viewModel);
        }

        private static void DisableInteractionColliders(GameObject root)
        {
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
            {
                c.enabled = false;
            }

            foreach (Collider2D c in root.GetComponentsInChildren<Collider2D>(true))
            {
                c.enabled = false;
            }
        }

        public bool CanAccept(DragPayload payload)
        {
            return payload.GetData<IGridNode>()?.ConfigData?.IsReturnable == true;
        }

        public bool OnDrop(DragPayload payload)
        {
            return CanAccept(payload);
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
