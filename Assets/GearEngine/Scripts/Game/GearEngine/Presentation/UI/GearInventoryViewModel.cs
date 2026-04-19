using System;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public GearInventoryViewModel(IGearEngineService engineService, IInventoryService inventoryService)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));

            InventoryModel = inventoryService.GetInventory();
            if (InventoryModel?.Items != null)
            {
                InventoryModel.Items.CollectionChanged += OnAvailableItemsChanged;
            }

            RefreshInventoryLabel();
        }

        public int MaxSlots => InventoryModel?.MaxSlots ?? 0;

        public int CurrentCount => InventoryModel?.Items.Count ?? 0;

        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private readonly IGearEngineService engineService;
        private readonly IInventoryService inventoryService;

        [ObservableProperty] private IItem selectedItem;

        [ObservableProperty] private InventoryModel inventoryModel;
        [ObservableProperty] private string inventoryLimitText;
        [ObservableProperty] private int inventoryListRevision;

        public void NotifySlotDragAccepted(GearConfigData gear)
        {
            try
            {
                if (gear == null)
                {
                    throw new ArgumentNullException(nameof(gear));
                }

                inventoryService?.TryConsume(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragAccepted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void SelectGearLocal(GearConfigData gear)
        {
            if (InventoryModel.Items.Contains(gear))
            {
                SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

        protected override void OnClosed()
        {
            if (InventoryModel?.Items != null)
            {
                InventoryModel.Items.CollectionChanged -= OnAvailableItemsChanged;
            }

            base.OnClosed();
        }

        private void OnAvailableItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshInventoryLabel();
            InventoryListRevision++;
        }

        private void RefreshInventoryLabel()
        {
            InventoryLimitText = $"Inventory: {CurrentCount}/{MaxSlots}";
        }
    }
}
