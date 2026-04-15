using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Presentation.UI;
using Scaffold.Events.Contracts;
using Scaffold.MVVM;
using VContainer;

namespace GearEngine.Race
{
    public sealed class RaceViewModel : ViewModel
    {
        public RaceViewModel(RaceStartData startData)
        {
            this.startData = startData ?? throw new ArgumentNullException(nameof(startData));
        }

        public BoardViewModel Board { get; } = new BoardViewModel();
        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();
        public TrackViewModel Track { get; private set; }

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        public IGearTrashService TrashService => trashService;
        public GearEngineFeatureToggleSO FeatureToggle => featureToggle;
        public IDragService DragService => dragService;

        private readonly RaceStartData startData;

        [Inject] private IGearEngineService engineService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private ITrackSimulationRunner trackSimulationRunner;
        
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
            ValidateStartData();

            BindChildViewModel(Board);
            BindChildViewModel(Inventory);

            int maxSlots = startData.GearEngineData?.MaxInventorySlots ?? 5;
            Inventory.Initialize(engineService, maxSlots, dragService);
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

            if (startData.GearEngineData != null)
            {
                if (startData.GearEngineData.BoardLayout != null)
                {
                    Board.LoadLayout(startData.GearEngineData.BoardLayout);
                }

                if (startData.GearEngineData.InventoryGears != null)
                {
                    Inventory.LoadInventory(startData.GearEngineData.InventoryGears);
                }
            }

            SetupTrack();
        }

        public void ToggleRace()
        {
            if (engineService == null || Track == null)
            {
                return;
            }

            if (engineService.IsRunning)
            {
                engineService.Stop();
                Track.Toggle(false);
            }
            else
            {
                engineService.Play();
                Track.Toggle(true);
            }
        }

        private void ValidateStartData()
        {
            if (startData.TrackDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.TrackDefinition is missing.");
            }

            if (startData.CarDefinition == null)
            {
                throw new InvalidOperationException("[RaceViewModel] RaceStartData.CarDefinition is missing.");
            }
        }

        private void SetupTrack()
        {
            TrackSimulation simulation = trackFactory.Create(startData.CarDefinition, startData.TrackDefinition, startData.SimulationConfig);
            trackSimulationRunner.SetSimulation(simulation);
            Track = new TrackViewModel(simulation);
            BindChildViewModel(Track);
        }
    }
}
