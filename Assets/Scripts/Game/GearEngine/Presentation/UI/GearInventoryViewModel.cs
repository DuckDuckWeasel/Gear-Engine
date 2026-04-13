using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private IGearEngineService engineService;

        [ObservableProperty]
        private GearInventoryModel inventoryModel = new GearInventoryModel();

        public event Action<Vector3, GearConfigData> OnGearDraggedToBoard;

        public void Initialize(IGearEngineService engineService)
        {
            this.engineService = engineService;
        }

        protected override void Initialize()
        {
        }

        public void LoadInventory(IEnumerable<GearConfig> gearConfigs)
        {
            if (gearConfigs == null)
            {
                throw new ArgumentNullException(nameof(gearConfigs));
            }

            foreach (GearConfig config in gearConfigs)
            {
                if (config == null)
                {
                    continue;
                }

                AddGearToInventory(config.CreateRuntimeData());
            }
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

        public void ConsumeSpecificGear(GearConfigData gearData)
        {
            if (gearData == null)
            {
                throw new ArgumentNullException(nameof(gearData));
            }

            int index = FindGearIndex(gearData);
            if (index < 0)
            {
                Debug.LogError("[GearInventoryViewModel] ConsumeSpecificGear: gear not found in inventory.");
                return;
            }

            RemoveGearAt(index);
        }

        public void NotifyGearDropped(Vector3 worldPos, GearConfigData gearData)
        {
            if (gearData == null)
            {
                throw new ArgumentNullException(nameof(gearData));
            }

            OnGearDraggedToBoard?.Invoke(worldPos, gearData);
        }

        public void SelectGearLocal(GearConfigData gear)
        {
            if (InventoryModel.AvailableGears.Contains(gear))
            {
                InventoryModel.SelectedGear = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

        private int FindGearIndex(GearConfigData gearData)
        {
            for (int i = 0; i < InventoryModel.AvailableGears.Count; i++)
            {
                if (ReferenceEquals(InventoryModel.AvailableGears[i], gearData))
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveGearAt(int index)
        {
            GearConfigData removed = InventoryModel.AvailableGears[index];
            InventoryModel.AvailableGears.RemoveAt(index);
            if (ReferenceEquals(InventoryModel.SelectedGear, removed))
            {
                InventoryModel.SelectedGear = null;
            }
        }
    }
}
