using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public GearInventoryViewModel(int maxInventorySlots, IReadOnlyList<GearConfig> inventoryGears, IGearEngineService engineService, IInventoryService inventoryService, IDragService dragService = null)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            this.dragService = dragService;

            InventoryModel = inventoryService.Model;
            if (InventoryModel?.AvailableItems != null)
            {
                InventoryModel.AvailableItems.CollectionChanged += OnAvailableItemsChanged;
            }

            inventoryService.Initialize(maxInventorySlots, inventoryGears);
            this.maxInventorySlots = inventoryService.MaxSlots;
            RefreshInventoryLabel();
        }

        public int MaxSlots => maxInventorySlots;

        public int CurrentCount => InventoryModel?.AvailableItems.Count ?? 0;

        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private readonly IDragService dragService;
        private readonly IGearEngineService engineService;
        private readonly IInventoryService inventoryService;
        private int maxInventorySlots = int.MaxValue;

        [ObservableProperty] private InventoryModel inventoryModel;
        [ObservableProperty] private string inventoryLimitText;
        [ObservableProperty] private int inventoryListRevision;

        public void NotifySlotDragStarted(GearConfigData gear)
        {
            try
            {
                dragService?.StartDrag(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragStarted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void NotifySlotDragEnded()
        {
            try
            {
                dragService?.EndDrag();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragEnded failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void NotifySlotDragAccepted(GearConfigData gear)
        {
            try
            {
                if (gear == null)
                {
                    throw new ArgumentNullException(nameof(gear));
                }

                inventoryService?.ConsumeSpecificItem(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragAccepted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void SelectGearLocal(GearConfigData gear)
        {
            if (InventoryModel.AvailableItems.Contains(gear))
            {
                InventoryModel.SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

        protected override void OnClosed()
        {
            if (InventoryModel?.AvailableItems != null)
            {
                InventoryModel.AvailableItems.CollectionChanged -= OnAvailableItemsChanged;
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
