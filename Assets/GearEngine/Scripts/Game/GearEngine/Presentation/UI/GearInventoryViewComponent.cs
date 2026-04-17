using System.Collections.Specialized;
using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.GearEngine.Visuals;
using UnityEngine;
using UnityEngine.Assertions;
using Scaffold.MVVM;
using GearEngine.GearEngine.Services.Inventory;
using TMPro;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryViewComponent : ViewComponent<GearInventoryViewModel>
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TagSO gridBoardTag;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private TextMeshProUGUI inventoryLimitLabel;
        
        private Transform boardReferenceTransform;

        protected override void OnBind()
        {
            if (viewModel.InventoryModel.AvailableItems != null)
            {
                viewModel.InventoryModel.AvailableItems.CollectionChanged += OnInventoryCollectionChanged;
            }

            Bind(() => viewModel.InventoryLimitText, () => inventoryLimitLabel.text);
            Bind<IItem, IItem>(() => viewModel.InventoryModel.SelectedItem, OnSelectionChanged);
            CheckTargetRect();
            DrawInitialList();
        }

        private void CheckTargetRect()
        {
            var frustumFit = GameObject.FindObjectOfType<GearEngine.Presentation.World.FrustumFit>();
            if (frustumFit != null)
            {
                boardReferenceTransform = frustumFit.transform;
            }
            else
            {
                // Fallback to searching by standard GearBoardViewComponent if no frustum mapping is used
                var boardView = GameObject.FindObjectOfType<BoardViewComponent>();
                if (boardView != null)
                {
                    boardReferenceTransform = boardView.transform;
                }
            }
        }

        private void DrawInitialList()
        {
            if (viewModel.InventoryModel.AvailableItems == null)
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

        private void OnInventoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildUIList();
        }

        private void RebuildUIList()
        {
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in viewModel.InventoryModel.AvailableItems)
            {
                // Create the slot object based on the generic Interface ID
                GameObject slotObj = CreateSlotObject(item);
                if (slotObj == null) continue;

                var dragger = slotObj.GetComponent<DragHandler>();
                if (dragger == null) dragger = slotObj.AddComponent<DragHandler>();

                var slotView = slotObj.GetComponent<GearInventorySlotView>();
                if (slotView == null) slotView = slotObj.AddComponent<GearInventorySlotView>();

                if (gridBoardTag != null) dragger.AddAcceptedTag(gridBoardTag);

                // Now attempt to resolve specific Visual definitions (requires Concrete object)
                if (item is GearConfigData gear)
                {
                    Transform visualContainer = slotObj.transform.Find("VisualContainer");
                    if (visualContainer == null)
                    {
                        visualContainer = slotObj.transform;
                    }

                    // Match pure physical screen space: force the SpriteRenderer lossyScale to equal the Grid's lossyScale
                    float baseScale = 56f;
                    if (boardReferenceTransform != null && visualContainer.lossyScale.x > 0f)
                    {
                        baseScale = boardReferenceTransform.lossyScale.x / visualContainer.lossyScale.x;
                    }
                    float totalScale = gear.RelativeScaleMultiplier * baseScale;

                    dragger.GhostScaleMultiplier = totalScale;

                    // Setup visual using shared utility
                    GameObject visualObj = GearVisualSetup.SetupVisual(visualContainer, gear, totalScale);
                    if (visualObj != null)
                    {
                        dragger.GhostPrefab = visualObj;
                    }

                    slotView.Bind(gear, viewModel);

                    // Wire drag lifecycle to centralized IDragService
                    if (viewModel.DragService != null)
                    {
                        GearConfigData capturedGear = gear;
                        dragger.OnDragBegin += () => viewModel.DragService.StartDrag(capturedGear);
                        dragger.OnDragEnd += () => viewModel.DragService.EndDrag();
                    }
                }
            }
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

        private void OnDestroy()
        {
            if (viewModel?.InventoryModel?.AvailableItems != null)
            {
                viewModel.InventoryModel.AvailableItems.CollectionChanged -= OnInventoryCollectionChanged;
            }
        }

        [ContextMenu("Mock: UI Click First Available Gear")]
        public void MockClickFirstGear()
        {
            if (viewModel.InventoryModel.AvailableItems.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.InventoryModel.AvailableItems.First() as GearConfigData);
            }
        }
    }
}
