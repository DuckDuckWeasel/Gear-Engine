using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Presentation.UI;
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

        public GearInventoryViewModel Inventory { get; } = new GearInventoryViewModel();

        public BoardViewModel Board { get; } = new BoardViewModel();

        public TrackViewModel Track { get; private set; }

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        private readonly RaceStartData startData;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private IGridManager gridManager;

        [Inject]
        private GearNodeFactory nodeFactory;

        [Inject]
        private BoardConfigSO boardConfig;

        [Inject]
        private TrackSimulationFactory trackFactory;

        [Inject]
        private ITrackSimulationRunner trackSimulationRunner;

        protected override void Initialize()
        {
            base.Initialize();
            ValidateStartData();
            SetupInventory();
            SetupBoard();
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

        private void SetupInventory()
        {
            BindChildViewModel(Inventory);
            Inventory.Initialize(engineService);

            GearEngineStartData gearData = startData.GearEngineData;
            if (gearData?.InventoryGears != null)
            {
                Inventory.LoadInventory(gearData.InventoryGears);
            }
        }

        private void SetupBoard()
        {
            BindChildViewModel(Board);
            Board.Initialize(engineService, gridManager, nodeFactory, boardConfig);

            GearEngineStartData gearData = startData.GearEngineData;
            if (gearData?.BoardLayout != null)
            {
                Board.LoadLayout(gearData.BoardLayout);
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
