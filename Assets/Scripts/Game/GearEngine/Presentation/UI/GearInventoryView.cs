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
                
                // HACK: Reflection fallback config assignment since EventSystems are used
                var tagObj = UnityEditor.AssetDatabase.LoadAssetAtPath<TagSO>("Assets/Game/GearEngine/Configs/GridBoard_Tag.asset");
                var so = new UnityEditor.SerializedObject(dragger);
                var acceptedTags = so.FindProperty("acceptedTargetTags");
                acceptedTags.arraySize = 1;
                acceptedTags.GetArrayElementAtIndex(0).objectReferenceValue = tagObj;
                
                var customSpr = gear.UIIcon;
                if (customSpr != null)
                {
                    so.FindProperty("ghostPrefab").objectReferenceValue = null; // Will fallback to itself
                }
                
                so.ApplyModifiedProperties();

                // Generate inner icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(slotGroup.transform, false);
                var iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = Vector2.zero;
                iconRt.anchorMax = Vector2.one;
                iconRt.sizeDelta = Vector2.zero;
                var iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
                
                if (customSpr != null) iconImg.sprite = customSpr;
                else iconImg.color = new Color(0.6f, 0.6f, 0.65f); // fallback color

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
