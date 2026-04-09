using UnityEngine;
using Scaffold.MVVM;
using Scaffold.MVVM.Contracts;
using Scaffold.MVVM.Binding;
using System.Collections.Specialized;
using System.Linq;

namespace Game.GearEngine.Presentation
{
    public class GearInventoryView : View<GearInventoryViewModel>
    {
        // Drag in references in Unity Inspector
        [SerializeField] private RectTransform itemsContainer;
        
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
            Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Canvas Renderer synced! Rendering {viewModel.InventoryModel.AvailableGears.Count} icons.");
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
            // Here we would pool or instantiate generic UI slots
            Debug.Log($"<color=#ff99aa>[GearInventoryView]</color> Reactive Layout rebuilt. Total items now: {viewModel.InventoryModel.AvailableGears.Count}");
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
