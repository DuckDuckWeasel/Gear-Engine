using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
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
        [Inject] private IBoardService boardService;
        [Inject] private IEventBus eventBus;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IInventoryService inventoryService;
        [Inject] private IGearPresentationTransferService presentationTransferService;
        [Inject] private IGearLoadoutService loadoutService;
        [Inject] private TrackSimulationFactory trackFactory;

        protected override void Initialize()
        {
            base.Initialize();

            engineService.ResetGridSimulationState();

            LapRaceSession previewSession = trackFactory.Create(trackService.CurrentCar, trackService.CurrentTrack);
            trackService.SetCurrentSession(previewSession);

            Track = new TrackViewModel(trackService.CurrentSession);
            BindChildViewModel(Track);

            Board = new BoardViewModel(boardService, inventoryService, engineService, dragService);
            BindChildViewModel(Board);

            if (loadoutService.HasSavedLoadout && !boardService.GetAllNodes().Any())
            {
                Board.LoadLayout(loadoutService.GetBoardLayout());
            }

            Inventory = new GearInventoryViewModel(engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        public void GoToRace()
        {
            try
            {
                BoardLayoutData snapshot = BoardLayoutData.FromNodes(boardService.GetAllNodes());
                loadoutService.SaveBoardLayout(snapshot);
                loadoutService.SaveInventoryGearConfigs(SnapshotInventoryGearConfigs());

                navigation.Open(new ActiveRaceViewModel(), closeCurrent: true);
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
            foreach (IItem item in inventoryService.GetInventory().Items)
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
