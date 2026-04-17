using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public int MaxSlots => maxInventorySlots;
        private int maxInventorySlots = int.MaxValue;

        public int CurrentCount => InventoryModel?.AvailableItems.Count ?? 0;
        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private IDragService dragService;
        private IGearEngineService engineService;
        private IInventoryService inventoryService;

        [ObservableProperty] private InventoryModel inventoryModel;
        [ObservableProperty] private string inventoryLimitText;

        private BoardViewModel board;

        public void Initialize(
            int maxInventorySlots,
            IReadOnlyList<GearConfig> inventoryGears,
            IGearEngineService engineService,
            IInventoryService inventoryService,
            BoardViewModel boardViewModel,
            IDragService dragService = null)
        {
            this.engineService = engineService;
            this.inventoryService = inventoryService;
            this.inventoryModel = inventoryService.Model;
            this.maxInventorySlots = inventoryService.MaxSlots;
            this.dragService = dragService;
            this.board = boardViewModel ?? throw new ArgumentNullException(nameof(boardViewModel));

            board.BoardGearReturnedToInventory += OnBoardGearReturnedToInventory;

            inventoryService.Model.AvailableItems.CollectionChanged += UpdateLabels;
            inventoryService.Initialize(maxInventorySlots, inventoryGears);
        }

        private void OnBoardGearReturnedToInventory(GearConfigData config)
        {
            try
            {
                if (config != null)
                {
                    inventoryService?.AddItem(config);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] OnBoardGearReturnedToInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void ConsumeGearFromTrash(GearConfigData gear)
        {
            try
            {
                if (gear == null)
                {
                    return;
                }

                inventoryService?.ConsumeSpecificItem(gear);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] ConsumeGearFromTrash failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateLabels(object sender, NotifyCollectionChangedEventArgs e)
        {
            InventoryLimitText = $"Inventory: {CurrentCount}/{MaxSlots}";
        }

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
            if (board != null)
            {
                board.BoardGearReturnedToInventory -= OnBoardGearReturnedToInventory;
            }

            base.OnClosed();
        }
    }
}
