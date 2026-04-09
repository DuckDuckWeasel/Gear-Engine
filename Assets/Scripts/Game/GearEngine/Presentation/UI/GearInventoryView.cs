using UnityEngine;
using Scaffold.MVVM;
using System.Collections.Specialized;
using System.Linq;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public class GearInventoryView : View<GearInventoryViewModel>
    {
        // Drag in references in Unity Inspector
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TagSO gridBoardTag;
        
        private IObjectResolver container;

        [Inject]
        public void Construct(GearInventoryViewModel vm, IObjectResolver container)
        {
            this.container = container;
            Bind(vm);
        }

        
        protected override void OnBind()
        {
            // Listen securely to standard C# Collection updates as part of MVVM fallback list behavior
            if (viewModel.InventoryModel.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged += OnInventoryCollectionChanged;
            }

            // Standard scalar binding to track user selection visual targets
            Bind<GearConfigData, GearConfigData>(() => viewModel.InventoryModel.SelectedGear, OnSelectionChanged);
            
            DrawInitialList();
        }

        private void DrawInitialList()
        {
            if (viewModel == null || viewModel.InventoryModel.AvailableGears == null) return;
            
            // Rebuild UI container dynamically for raw test usage
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
            if (itemsContainer == null || viewModel == null) return;

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var gear in viewModel.InventoryModel.AvailableGears)
            {
                GameObject slotGroup = new GameObject($"Slot_{gear.Id}");
                slotGroup.transform.SetParent(itemsContainer, false);
                
                var rect = slotGroup.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(120, 120);

                var img = slotGroup.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // Background shade

                // Just create drag logic
                var dragger = slotGroup.AddComponent<DragHandler>();
                var slotView = slotGroup.AddComponent<GearInventorySlotView>();
                
                if (container != null) 
                {
                    container.Inject(slotView);
                    container.Inject(dragger); // Good practice to inject dragger too in case it needs it later
                }
                
                if (gridBoardTag != null)
                {
                    dragger.AddAcceptedTag(gridBoardTag);
                }
                dragger.GhostScaleMultiplier = gear.UIScaleMultiplier; 

                // Generate inner visual purely from the VisualPrefab!
                if (gear.VisualPrefab != null)
                {
                    GameObject visualObj = Instantiate(gear.VisualPrefab, slotGroup.transform);
                    visualObj.name = "VisualInstance";
                    visualObj.transform.localPosition = new Vector3(0, 0, -10f); // Float it above the background
                    
                    // Canvas Overlay scaling (1 world unit = 1 UI pixel). 
                    // Use data-driven UIScaleMultiplier instead of hardcoded 85f overlay stretch
                    visualObj.transform.localScale = new Vector3(gear.UIScaleMultiplier, gear.UIScaleMultiplier, gear.UIScaleMultiplier);

                    // Ensure raw SpriteRenderers draw over the UI Canvas Background!
                    var srs = visualObj.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach(var sr in srs)
                    {
                        sr.sortingOrder = 50; 
                    }

                    // --- Add the missing central type icon for the Inventory Visual ---
                    if (gear.UIIcon != null)
                    {
                        GameObject iconObj = new GameObject("InventoryUIIcon");
                        iconObj.transform.SetParent(visualObj.transform, false);
                        iconObj.transform.localPosition = new Vector3(0, 0, -1f); // Float in front of the base sprite
                        iconObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Match the GearView relative scale

                        SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
                        iconRenderer.sprite = gear.UIIcon;
                        iconRenderer.sortingOrder = 55; // Higher than the base sprite (50)

                        Shader fillShader = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
                        if (fillShader != null)
                        {
                            Material mat = new Material(fillShader);
                            // Set to 1f so it looks fully "lit" in the inventory instead of grayed out
                            mat.SetFloat("_FillAmount", 1f); 
                            iconRenderer.material = mat;
                        }
                    }

                    // Feed the completely decorated visual instance back as the ghost blueprint to DragHandler!
                    dragger.GhostPrefab = visualObj; 
                }

                slotView.Bind(gear, viewModel);
            }
        }
        
        private void OnDestroy()
        {
            if (viewModel?.InventoryModel?.AvailableGears != null)
            {
                viewModel.InventoryModel.AvailableGears.CollectionChanged -= OnInventoryCollectionChanged;
            }
        }

        // --- Mocking UI Clicks for the moment ---
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
