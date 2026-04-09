using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Presentation
{
    public partial class GearInventoryViewModel : ViewModel
    {
        private IGridManager gridManager;

        [ObservableProperty]
        private GearInventoryModel inventoryModel = new GearInventoryModel();

        [Inject]
        public void Construct(IGridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public bool CanDrag => gridManager != null && !gridManager.IsRunning;

        protected override void Initialize()
        {
            // Setup bindings if necessary, Scaffold architecture usually binds nested properties
            // Automatically registers via Source Generators / Reflection in the base class.
        }

        public void AddGearToInventory(GearConfigData gear)
        {
            if (gear == null) return;
            InventoryModel.AvailableGears.Add(gear);
        }

        public bool TryConsumeSelectedGear()
        {
            if (InventoryModel.SelectedGear == null) return false;
            
            bool success = InventoryModel.AvailableGears.Remove(InventoryModel.SelectedGear);
            if (success)
            {
                InventoryModel.SelectedGear = null;
            }
            return success;
        }

        public bool ConsumeSpecificGear(GearConfigData gearData)
        {
            if (gearData == null) return false;
            // Ensure the UI model legally supports removing this specific gear item dragged.
            bool success = InventoryModel.AvailableGears.Remove(gearData);
            return success;
        }

        public void SelectGearLocal(GearConfigData gear)
        {
            if (InventoryModel.AvailableGears.Contains(gear))
            {
                InventoryModel.SelectedGear = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }
    }
}
