using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public int MaxSlots => maxInventorySlots;
        private int maxInventorySlots = int.MaxValue;

        public IDragService DragService => dragService;
        private IDragService dragService;

        public int CurrentCount => InventoryModel?.AvailableItems.Count ?? 0;
        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private IGearEngineService engineService;
        private IInventoryService inventoryService;

        [ObservableProperty] private InventoryModel inventoryModel;
        [ObservableProperty] private string inventoryLimitText;

        public event Action<Vector3, GearConfigData> OnGearDraggedToBoard;

        public void Initialize(int maxInventorySlots, IReadOnlyList<GearConfig> inventoryGears, IGearEngineService engineService, IInventoryService inventoryService, IDragService dragService = null)
        {
            this.engineService = engineService;
            this.inventoryService = inventoryService;
            this.inventoryModel = inventoryService.Model;
            this.maxInventorySlots = inventoryService.MaxSlots;
            this.dragService = dragService;

            inventoryService.Model.AvailableItems.CollectionChanged += UpdateLabels;
            inventoryService.Initialize(maxInventorySlots, inventoryGears);
        }

        private void UpdateLabels(object sender, NotifyCollectionChangedEventArgs e)
        {
            InventoryLimitText = $"Inventory: {CurrentCount}/{MaxSlots}";
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
            if (InventoryModel.AvailableItems.Contains(gear))
            {
                InventoryModel.SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

    }
}
