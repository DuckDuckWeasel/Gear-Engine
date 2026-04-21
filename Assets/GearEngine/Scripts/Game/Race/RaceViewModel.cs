using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
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

        public GearInventoryViewModel Inventory { get; private set; }

        public BoardViewModel Board { get; private set; }

        public TrackViewModel Track { get; private set; }

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        private readonly RaceStartData startData;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private TrackSimulationFactory trackFactory;

        [Inject]
        private RaceManagerService raceManager;

        [Inject]
        private IInventoryService inventoryService;

        [Inject]
        private IBoardService boardService;

        [Inject]
        private SplineCarRunnerService aiRunner;

        protected override void Initialize()
        {
            base.Initialize();
            ValidateStartData();
            SetupBoard();
            SetupInventory();
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

        private void SetupBoard()
        {
            Board = new BoardViewModel(boardService, engineService, inventoryService);
            BindChildViewModel(Board);
        }

        private void SetupInventory()
        {
            Inventory = new GearInventoryViewModel(engineService, boardService, inventoryService);
            BindChildViewModel(Inventory);
        }

        private void SetupTrack()
        {
            RaceState session = trackFactory.Create(startData.CarDefinition, startData.TrackDefinition, startData.SessionConfig);
            raceManager.RegisterRace(session);
            Track = new TrackViewModel(session, raceManager, aiRunner, trackFactory);
            BindChildViewModel(Track);
        }
    }
}
