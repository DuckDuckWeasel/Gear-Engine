using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Merge;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineViewModel : ViewModel
    {
        public GearEngineViewModel(GearEngineStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        public BoardViewModel Board { get; private set; }
        public GearInventoryViewModel Inventory { get; private set; }
        public TrashZoneViewModel TrashZone { get; private set; }

        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;

        private readonly GearEngineStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private IGearNodeFactory nodeFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IGridSwapService swapService;
        [Inject] private IGridMergeService mergeService;
        [Inject] private IInventoryService inventoryService;

        [ObservableProperty] private bool isRunning = false;

        protected override void Initialize()
        {
            base.Initialize();

            Board = new BoardViewModel();
            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig, eventBus, featureToggle, dragService, swapService, mergeService);

            Inventory = new GearInventoryViewModel();
            Inventory.Initialize(startData.MaxInventorySlots, startData.InventoryGears, engineService, inventoryService, dragService);

            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, inventoryService, eventBus, featureToggle);

            BindChildViewModel(Board);
            BindChildViewModel(Inventory);
            BindChildViewModel(TrashZone);

            Board.OnGearReturnRequested += ReturnGearToInventory;

            if (startData.BoardLayout != null)
            {
                Board.LoadLayout(startData.BoardLayout);
            }
        }

        private void ReturnGearToInventory(GearConfigData config)
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
                Debug.LogError($"[GearEngineViewModel] ReturnGearToInventory failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        internal void ToggleSimulation()
        {
            try
            {
                if (gridManager == null)
                {
                    throw new InvalidOperationException("Grid manager is not available.");
                }

                if (gridManager.IsRunning)
                {
                    gridManager.Stop();
                }
                else
                {
                    gridManager.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearEngineViewModel] ToggleSimulation failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
