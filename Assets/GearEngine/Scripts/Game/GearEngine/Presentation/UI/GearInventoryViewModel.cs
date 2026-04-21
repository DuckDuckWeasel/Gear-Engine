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

        public ObservableCollection<GearConfigData> TrayItems { get; } = new ObservableCollection<GearConfigData>();

        public bool CanDrag => engineService != null && !engineService.IsRunning;

        private readonly IGearEngineService engineService;
        private readonly IBoardService boardService;
        private readonly IInventoryService inventoryService;

        [ObservableProperty] private GearConfigData selectedItem;

        [ObservableProperty] private int inventoryListRevision;

        public void NotifySlotDragAccepted(GearConfigData gear)
        {
            try
            {
                if (gear == null)
                {
                    throw new ArgumentNullException(nameof(gear));
                }

                GearConfig source = gear.SourceGearConfig;
                if (source == null)
                {
                    Debug.LogError("[GearInventoryViewModel] NotifySlotDragAccepted: gear has no SourceGearConfig.");
                    return;
                }

                if (inventoryService.TryRemove(source))
                {
                    RebuildTray();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearInventoryViewModel] NotifySlotDragAccepted failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void SelectGearLocal(GearConfigData gear)
        {
            if (gear != null && TrayItems.Contains(gear))
            {
                SelectedItem = gear;
                Debug.Log($"<color=#aaaaff>[UI_ViewModel]</color> Player selected: {gear.Id}");
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
            List<GearConfig> placed = boardService.GetAllNodes()
                .Select(n => n.ConfigData?.SourceGearConfig)
                .Where(c => c != null)
                .ToList();

            Dictionary<GearConfig, int> ownedCounts = inventoryService.Owned
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (GearConfig p in placed)
            {
                if (ownedCounts.TryGetValue(p, out int n) && n > 0)
                {
                    ownedCounts[p] = n - 1;
                }
            }

            TrayItems.Clear();
            foreach (KeyValuePair<GearConfig, int> kv in ownedCounts)
            {
                for (int i = 0; i < kv.Value; i++)
                {
                    TrayItems.Add(kv.Key.CreateRuntimeData());
                }
            }

            InventoryListRevision++;
        }
    }
}
