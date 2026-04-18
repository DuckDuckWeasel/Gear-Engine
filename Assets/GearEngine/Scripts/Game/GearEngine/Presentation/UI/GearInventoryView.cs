using System.Collections.Specialized;
using System.Linq;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.GearEngine.Visuals;
using UnityEngine;
using Scaffold.MVVM;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class GearInventoryView : ViewComponent<GearInventoryViewModel>
    {
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TagSO gridBoardTag;
        [SerializeField] private GameObject slotPrefab;

        private IDragService dragService;
        private Transform boardReferenceTransform;

        public void SetDragService(IDragService dragService)
        {
            this.dragService = dragService;
        }

        public void SetBoardReference(Transform boardTransform)
        {
            boardReferenceTransform = boardTransform;
        }

        protected override void OnBind()
        {
            if (viewModel.InventoryModel.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged += OnInventoryCollectionChanged;
            }

            Bind<GearConfigData, GearConfigData>(() => viewModel.InventoryModel.SelectedGear, OnSelectionChanged);

            DrawInitialList();
        }

        private void DrawInitialList()
        {
            if (viewModel == null || viewModel.InventoryModel.AvailableGears == null)
            {
                return;
            }

            if (itemsContainer == null)
            {
                Debug.LogWarning($"<color=#ff5555>[GearInventoryView]</color> ItemsContainer is not assigned in the inspector!");
                return;
            }

            RebuildUIList();
        }

        private void OnSelectionChanged(GearConfigData newSelection)
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

        private void OnInventoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildUIList();
        }

        private void RebuildUIList()
        {
            if (itemsContainer == null || viewModel == null)
            {
                return;
            }

            ClearSlots();
            BuildSlots();
        }

        private void ClearSlots()
        {
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void BuildSlots()
        {
            foreach (GearConfigData gear in viewModel.InventoryModel.AvailableGears)
            {
                BuildSlot(gear);
            }
        }

        private void BuildSlot(GearConfigData gear)
        {
            GameObject slotObj = CreateSlotObject(gear);
            if (slotObj == null)
            {
                return;
            }

            DragHandler dragger = GetOrAddDragHandler(slotObj);
            GearInventorySlotView slotView = GetOrAddSlotView(slotObj);
            ConfigureDragTarget(dragger);
            ConfigureSlotVisual(slotObj, dragger, gear);
            slotView.Bind(gear, viewModel);
            SubscribeDragEvents(dragger, gear);
        }

        private GameObject CreateSlotObject(GearConfigData gear)
        {
            if (slotPrefab == null)
            {
                Debug.LogError($"[GearInventoryView] slotPrefab is missing! Cannot create slot for '{gear.Id}'. Please assign the GearSlot prefab in the inspector.");
                return null;
            }

            GameObject slot = Instantiate(slotPrefab, itemsContainer);
            slot.name = $"Slot_{gear.Id}";
            return slot;
        }

        private DragHandler GetOrAddDragHandler(GameObject slotObj)
        {
            DragHandler dragger = slotObj.GetComponent<DragHandler>();
            if (dragger == null)
            {
                dragger = slotObj.AddComponent<DragHandler>();
            }

            return dragger;
        }

        private GearInventorySlotView GetOrAddSlotView(GameObject slotObj)
        {
            GearInventorySlotView slotView = slotObj.GetComponent<GearInventorySlotView>();
            if (slotView == null)
            {
                slotView = slotObj.AddComponent<GearInventorySlotView>();
            }

            return slotView;
        }

        private void ConfigureDragTarget(DragHandler dragger)
        {
            if (gridBoardTag != null)
            {
                dragger.AddAcceptedTag(gridBoardTag);
            }
        }

        private void ConfigureSlotVisual(GameObject slotObj, DragHandler dragger, GearConfigData gear)
        {
            Transform visualContainer = GetVisualContainer(slotObj.transform);
            float totalScale = CalculateSlotScale(gear, visualContainer);
            dragger.GhostScaleMultiplier = totalScale;
            GameObject visualObj = GearVisualSetup.SetupVisual(visualContainer, gear, totalScale);
            if (visualObj != null)
            {
                dragger.GhostPrefab = visualObj;
            }
        }

        private Transform GetVisualContainer(Transform slotTransform)
        {
            Transform visualContainer = slotTransform.Find("VisualContainer");
            return visualContainer ?? slotTransform;
        }

        private float CalculateSlotScale(GearConfigData gear, Transform visualContainer)
        {
            float baseScale = 56f;
            if (boardReferenceTransform != null && visualContainer.lossyScale.x > 0f)
            {
                baseScale = boardReferenceTransform.lossyScale.x / visualContainer.lossyScale.x;
            }

            return gear.RelativeScaleMultiplier * baseScale;
        }

        private void SubscribeDragEvents(DragHandler dragger, GearConfigData gear)
        {
            if (dragService == null)
            {
                return;
            }

            GearConfigData capturedGear = gear;
            dragger.OnDragBegin += () => dragService.StartDrag(capturedGear);
            dragger.OnDragEnd += () => dragService.EndDrag();
        }

        private void OnDestroy()
        {
            if (viewModel?.InventoryModel?.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged -= OnInventoryCollectionChanged;
            }
        }

        [ContextMenu("Mock: UI Click First Available Gear")]
        public void MockClickFirstGear()
        {
            if (viewModel.InventoryModel.AvailableGears.Count > 0)
            {
                viewModel.SelectGearLocal(viewModel.InventoryModel.AvailableGears.First());
            }
        }
    }
}
