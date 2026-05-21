using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class GearInventoryViewModel : ViewModel
    {
        public GearInventoryViewModel(IGearEngineService engineService, IBoardService boardService, IInventoryService inventoryService)
        {
            this.engineService = engineService ?? throw new ArgumentNullException(nameof(engineService));
            this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
            this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));

            inventoryService.InventoryChanged += OnInventoryChanged;
            boardService.GearPlaced += OnBoardGearPlaced;
            boardService.GearRemoved += OnBoardGearRemoved;

            RebuildTray();
        }

        public ObservableCollection<GearItemData> TrayItems { get; } = new ObservableCollection<GearItemData>();

        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private readonly IGearEngineService engineService;
        private readonly IBoardService boardService;
        private readonly IInventoryService inventoryService;

        [ObservableProperty] private GearItemData selectedItem;

        [ObservableProperty] private int inventoryListRevision;

        public void NotifySlotDragAccepted(GearItemData gear)
        {
            try
            {
                if (gear == null)
                {
                    throw new ArgumentNullException(nameof(gear));
                }

                if (gear.Owner == null)
                {
                    Debug.LogError("[GearInventoryViewModel] NotifySlotDragAccepted: gear has no Owner.");
                    return;
                }

                // Keep OwnedGear in inventory while it is on the board; RebuildTray hides placed instances in the tray.
                RebuildTray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragAccepted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void SelectGearLocal(GearItemData gear)
        {
            if (gear != null && TrayItems.Contains(gear))
            {
                SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
            }
        }

        public event Action<GearItemData> OnInventoryClicked;

        internal void HandleInventoryClick(GearItemData gear)
        {
            if (gear != null)
            {
                OnInventoryClicked?.Invoke(gear);
            }
        }

        protected override void OnClosed()
        {
            inventoryService.InventoryChanged -= OnInventoryChanged;
            boardService.GearPlaced -= OnBoardGearPlaced;
            boardService.GearRemoved -= OnBoardGearRemoved;
            base.OnClosed();
        }

        private void OnInventoryChanged()
        {
            RebuildTray();
        }

        private void OnBoardGearPlaced(IGridNode _)
        {
            RebuildTray();
        }

        private void OnBoardGearRemoved(IGridNode _)
        {
            RebuildTray();
        }

        private void RebuildTray()
        {
            var placed = new HashSet<OwnedGear>(boardService.GetAllNodes()
                .Select(n => n.ConfigData?.Owner)
                .Where(o => o != null));

            TrayItems.Clear();
            foreach (OwnedGear o in inventoryService.Owned)
            {
                if (placed.Contains(o))
                {
                    continue;
                }

                GearItemData data = o.Config.CreateRuntimeData();
                data.Owner = o;
                TrayItems.Add(data);
            }

            InventoryListRevision++;
        }
    }
}
