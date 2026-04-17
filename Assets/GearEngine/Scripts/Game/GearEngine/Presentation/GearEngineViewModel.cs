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

        [ObservableProperty]
        private bool isSimulationRunning;

        private readonly GearEngineStartData startData;

        private BoardViewModel board;
        private GearInventoryViewModel inventory;
        private TrashZoneViewModel trashZone;

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

            board = new BoardViewModel();
            board.Initialize(
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

            inventory = new GearInventoryViewModel();
            inventory.Initialize(
                startData.MaxInventorySlots,
                startData.InventoryGears,
                engineService,
                inventoryService,
                board,
                dragService);

            trashZone = new TrashZoneViewModel(dragService, engineService, board, inventory, eventBus, featureToggle);

            Bind(() => board.IsSimulationRunning, () => IsSimulationRunning);
            IsSimulationRunning = board.IsSimulationRunning;

            BindChildViewModel(board);
            BindChildViewModel(inventory);
            BindChildViewModel(trashZone);
        }

        /// <summary>
        /// Binds child views to this screen's view models. Keeps child VMs off the public surface.
        /// </summary>
        internal void BindSubPresentation(
            BoardViewComponent boardView,
            GearInventoryViewComponent inventoryView,
            TrashDropZoneViewComponent trashDropZone)
        {
            boardView?.Bind(board);
            inventoryView?.SetBoardScaleReference(boardView != null ? boardView.transform : null);
            inventoryView?.Bind(inventory);
            trashDropZone?.Bind(trashZone);
            trashDropZone?.ApplyInitialPlacement();
        }

        internal void ToggleSimulation()
        {
            board?.ToggleSimulation();
        }
    }
}
