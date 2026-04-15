using System;
using GearEngine.CarSimulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using VContainer;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearEngineViewModel : ViewModel
    {
        public GearEngineViewModel(GearEngineStartData startData, TrackSimulation simulation)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }
        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;
        public IGearTrashService TrashService => trashService;
        public IDragService DragService => dragService;
        public SimulationControlViewModel SimControl { get; } = new SimulationControlViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public BoardViewModel Board { get; } = new BoardViewModel();

        private readonly GearEngineStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private IGridManager gridManager;
        [Inject] private GearNodeFactory nodeFactory;
        [Inject] private BoardConfigSO boardConfig;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IGearTransferService transferService;
        [Inject] private IGearTrashService trashService;

        protected override void Initialize()
        {
            base.Initialize();

            BindChildViewModel(SimControl);
            BindChildViewModel(Inventory);
            BindChildViewModel(Board);

            SimControl.Initialize(engineService);
            Inventory.Initialize(engineService, startData.MaxInventorySlots, dragService);

            if (startData.InventoryGears != null)
            {
                Inventory.LoadInventory(startData.InventoryGears);
            }

            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig, eventBus, featureToggle, dragService);

            if (transferService != null)
            {
                transferService.LinkBoard(Board);
                transferService.LinkInventory(Inventory);
            }

            if (trashService != null)
            {
                trashService.LinkedBoard = Board;
                trashService.LinkedInventory = Inventory;
            }

            if (startData.BoardLayout != null)
            {
                Board.LoadLayout(startData.BoardLayout);
            }
        }
    }
}