using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
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

        public TrashZoneViewModel TrashZone { get; private set; }

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        internal IDragService DragService => dragService;

        private readonly RaceStartData startData;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private IBoardService boardService;

        [Inject]
        private TrackSimulationFactory trackFactory;

        [Inject]
        private IRaceSessionRunner raceSessionRunner;

        [Inject]
        private IInventoryService inventoryService;

        [Inject]
        private IDragService dragService;

        [Inject]
        private IGearPresentationTransferService presentationTransferService;

        [Inject]
        private GearEngineFeatureToggleSO featureToggle;

        protected override void Initialize()
        {
            base.Initialize();
            ValidateStartData();
            SetupInventory();
            SetupBoard();
            SetupTrashZone();
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
            Inventory = new GearInventoryViewModel(engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);
        }

        private void SetupBoard()
        {
            Board = new BoardViewModel(boardService, inventoryService, engineService, dragService);
            BindChildViewModel(Board);
        }

        private void SetupTrashZone()
        {
            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        private void SetupTrack()
        {
            LapRaceSession session = trackFactory.Create(startData.CarDefinition, startData.TrackDefinition, startData.SessionConfig);
            raceSessionRunner.SetSession(session);
            Track = new TrackViewModel(session, spawnCarOnBindIfNoChild: false, spawnCarWhenSessionStartsRunning: true);
            BindChildViewModel(Track);
        }
    }
}
