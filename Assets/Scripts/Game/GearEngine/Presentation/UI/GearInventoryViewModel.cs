using CommunityToolkit.Mvvm.ComponentModel;
using Game.GearEngine;
using Scaffold.MVVM;
using UnityEngine;

namespace Game.GearEngine.Presentation
{
    public partial class GearInventoryViewModel : ViewModel
    {
        private IGearEngineService engineService;

        [ObservableProperty]
        private GearInventoryModel inventoryModel = new GearInventoryModel();

        public bool CanDrag => engineService != null && !engineService.IsRunning;

        public void Initialize(IGearEngineService engineService)
        {
            this.engineService = engineService;
        }

        protected override void Initialize()
        {
        }

        public void AddGearToInventory(GearConfigData gear)
        {
            if (gear == null)
            {
                return;
            }

            InventoryModel.AvailableGears.Add(gear);
        }

        public bool TryConsumeSelectedGear()
        {
            if (InventoryModel.SelectedGear == null)
            {
                return false;
            }

            bool success = InventoryModel.AvailableGears.Remove(InventoryModel.SelectedGear);
            if (success)
            {
                InventoryModel.SelectedGear = null;
            }

            return success;
        }

        public bool ConsumeSpecificGear(GearConfigData gearData)
        {
            if (gearData == null)
            {
                return false;
            }

            return InventoryModel.AvailableGears.Remove(gearData);
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
