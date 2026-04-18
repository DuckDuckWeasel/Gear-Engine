using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Merge;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Inventory;
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

        public GearInventoryViewModel Inventory { get; private set; }

        public BoardViewModel Board { get; private set; }

        public TrackViewModel Track { get; private set; }

        public bool IsRaceRunning => engineService?.IsRunning ?? false;

        private readonly RaceStartData startData;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private IGridManager gridManager;

        [Inject]
        private IGearNodeFactory nodeFactory;

        [Inject]
        private BoardConfigSO boardConfig;

        [Inject]
        private TrackSimulationFactory trackFactory;

        [Inject]
        private IRaceSessionRunner raceSessionRunner;

        [Inject]
        private IInventoryService inventoryService;

        [Inject]
        private IDragService dragService;

        [Inject]
        private IEventBus eventBus;

        [Inject]
        private GearEngineFeatureToggleSO featureToggle;

        [Inject]
        private IGridSwapService swapService;

        [Inject]
        private IGridMergeService mergeService;

        [Inject]
        private IGearPresentationTransferService presentationTransfer;

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
            GearEngineStartData gearData = startData.GearEngineData ?? new GearEngineStartData();
            Inventory = new GearInventoryViewModel(gearData.MaxInventorySlots, gearData.InventoryGears, engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);
        }

        private void SetupBoard()
        {
            GearEngineStartData gearData = startData.GearEngineData;
            Board = new BoardViewModel(engineService, gridManager, nodeFactory, boardConfig, presentationTransfer, eventBus, featureToggle, dragService, swapService, mergeService, gearData?.BoardLayout);
            BindChildViewModel(Board);
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
