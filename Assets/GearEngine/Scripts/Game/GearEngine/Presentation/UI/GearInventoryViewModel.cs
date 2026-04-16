using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private IGearEngineService engineService;
        private int maxInventorySlots = int.MaxValue;

        private IDragService dragService;
        public IDragService DragService => dragService;

        [ObservableProperty]
        private InventoryModel inventoryModel;

        public event Action<Vector3, GearConfigData> OnGearDraggedToBoard;

        public int CurrentCount => InventoryModel?.AvailableItems.Count ?? 0;
        public int MaxSlots => maxInventorySlots;

        public void Initialize(IGearEngineService engineService, IInventoryService inventoryService, IDragService dragService = null)
        {
            this.engineService = engineService;
            this.inventoryModel = inventoryService.Model;
            this.maxInventorySlots = inventoryService.MaxSlots;
            this.dragService = dragService;
        }

        protected override void Initialize()
        {
        }

        // State mutations delegated down to backend inventoryService.

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
            if (InventoryModel.AvailableItems.Contains(gear))
            {
                InventoryModel.SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

    }
}
