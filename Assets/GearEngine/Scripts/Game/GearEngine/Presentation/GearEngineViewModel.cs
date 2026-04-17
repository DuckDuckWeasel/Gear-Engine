using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.GearEngine;
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

        [ObservableProperty] private bool isSimulationRunning;

        private readonly GearEngineStartData startData;

        internal BoardViewModel Board;
        internal GearInventoryViewModel Inventory;
        internal TrashZoneViewModel TrashZone;

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

        protected override void Initialize()
        {
            base.Initialize();

            Board = new BoardViewModel();
            Board.Initialize(
                engineService,
                gridManager,
                nodeFactory,
                boardConfig,
                eventBus,
                featureToggle,
                dragService,
                swapService,
                mergeService,
                startData.BoardLayout);
            BindChildViewModel(Board);

            Inventory = new GearInventoryViewModel();
            Inventory.Initialize(
                startData.MaxInventorySlots,
                startData.InventoryGears,
                engineService,
                inventoryService,
                Board,
                dragService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, Inventory, eventBus, featureToggle);
            BindChildViewModel(TrashZone);

            Bind(() => Board.IsSimulationRunning, () => IsSimulationRunning);
        }

        internal void ToggleSimulation()
        {
            Board?.ToggleSimulation();
        }
    }
}
