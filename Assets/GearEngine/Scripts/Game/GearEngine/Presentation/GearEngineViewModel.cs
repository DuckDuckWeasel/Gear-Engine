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
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public partial class GearEngineViewModel : ViewModel
    {
        public GearEngineViewModel(GearEngineStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        internal IDragService DragService => dragService;

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
        [Inject] private IGearPresentationTransferService presentationTransferService;

        protected override void Initialize()
        {
            base.Initialize();
            Board = CreateBoard();
            BindChildViewModel(Board);
            Inventory = CreateInventory();
            BindChildViewModel(Inventory);
            TrashZone = CreateTrashZone();
            BindChildViewModel(TrashZone);
            Bind(() => Board.IsSimulationRunning, () => IsSimulationRunning);
        }

        private BoardViewModel CreateBoard()
        {
            return new BoardViewModel(engineService, gridManager, nodeFactory, boardConfig, presentationTransferService, eventBus, featureToggle, dragService, swapService, mergeService, startData.BoardLayout);
        }

        private GearInventoryViewModel CreateInventory()
        {
            return new GearInventoryViewModel(startData.MaxInventorySlots, startData.InventoryGears, engineService, inventoryService, dragService);
        }

        private TrashZoneViewModel CreateTrashZone()
        {
            return new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
        }

        internal void ToggleSimulation()
        {
            Board?.ToggleSimulation();
        }
    }
}
