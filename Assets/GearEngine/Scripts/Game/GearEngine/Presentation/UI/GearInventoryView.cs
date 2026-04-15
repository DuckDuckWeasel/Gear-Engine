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

        private Transform boardReferenceTransform;

        protected override void OnBind()
        {
            if (viewModel.InventoryModel.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged += OnInventoryCollectionChanged;
            }

            Bind<GearConfigData, GearConfigData>(() => viewModel.InventoryModel.SelectedGear, OnSelectionChanged);

            var frustumFit = GameObject.FindObjectOfType<GearEngine.Presentation.World.FrustumFit>();
            if (frustumFit != null)
            {
                boardReferenceTransform = frustumFit.transform;
            }
            else
            {
                // Fallback to searching by standard GearBoardView if no frustum mapping is used
                var boardView = GameObject.FindObjectOfType<BoardView>();
                if (boardView != null)
                {
                    boardReferenceTransform = boardView.transform;
                }
            }

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

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var gear in viewModel.InventoryModel.AvailableGears)
            {
                GameObject slotObj = CreateSlotObject(gear);
                if (slotObj == null)
                {
                    continue;
                }

                var dragger = slotObj.GetComponent<DragHandler>();
                if (dragger == null)
                {
                    dragger = slotObj.AddComponent<DragHandler>();
                }

                var slotView = slotObj.GetComponent<GearInventorySlotView>();
                if (slotView == null)
                {
                    slotView = slotObj.AddComponent<GearInventorySlotView>();
                }

                if (gridBoardTag != null)
                {
                    dragger.AddAcceptedTag(gridBoardTag);
                }

                // Locate containment boundary
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
