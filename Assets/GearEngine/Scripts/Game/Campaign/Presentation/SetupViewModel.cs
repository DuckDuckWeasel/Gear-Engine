using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
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
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class SetupViewModel : ViewModel
    {
        public TrackViewModel Track { get; private set; }
        public BoardViewModel Board { get; private set; }
        public GearInventoryViewModel Inventory { get; private set; }
        public TrashZoneViewModel TrashZone { get; private set; }

        internal IDragService DragService => dragService;

        [Inject] private ITrackService trackService;
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
        [Inject] private GearEngineStartData campaignGearStartData;
        [Inject] private IGearLoadoutService loadoutService;
        [Inject] private TrackSimulationFactory trackFactory;
        [Inject] private IRaceSessionRunner raceSessionRunner;

        protected override void Initialize()
        {
            base.Initialize();

            engineService.ResetGridSimulationState();

            LapRaceSession previewSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack);
            trackService.SetCurrentSession(previewSession);
            raceSessionRunner.SetSession(previewSession);

            GearEngineStartData gearStart = campaignGearStartData ?? new GearEngineStartData();

            Track = new TrackViewModel(trackService.CurrentSession);
            BindChildViewModel(Track);

            Board = new BoardViewModel(engineService, gridManager, nodeFactory, boardConfig, presentationTransferService, eventBus, featureToggle, dragService, swapService, mergeService, initialLayout: null);
            BindChildViewModel(Board);

            if (loadoutService.HasSavedLoadout && !gridManager.GetAllNodes().Any())
            {
                Board.LoadLayout(loadoutService.GetBoardLayout());
            }

            IReadOnlyList<GearConfig> inventorySeed = loadoutService.HasSavedInventory
                ? loadoutService.GetInventoryGearConfigs()
                : gearStart.InventoryGears;

            Inventory = new GearInventoryViewModel(gearStart.MaxInventorySlots, inventorySeed, engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        public void GoToRace()
        {
            try
            {
                BoardLayoutData snapshot = BoardLayoutData.FromNodes(gridManager.GetAllNodes());
                loadoutService.SaveBoardLayout(snapshot);
                loadoutService.SaveInventoryGearConfigs(SnapshotInventoryGearConfigs());

                navigation.Open(new ActiveRaceViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupViewModel] GoToRace failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void ReturnClicked()
        {
            try
            {
                navigation.Return();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SetupViewModel] ReturnToMainMenu failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private IReadOnlyList<GearConfig> SnapshotInventoryGearConfigs()
        {
            var list = new List<GearConfig>();
            foreach (IItem item in inventoryService.Model.AvailableItems)
            {
                if (item is GearConfigData data && data.SourceGearConfig != null)
                {
                    list.Add(data.SourceGearConfig);
                }
            }

            return list;
        }
    }
}
