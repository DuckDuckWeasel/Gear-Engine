using System;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Presentation;
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

        protected override void Initialize()
        {
            base.Initialize();

            GearEngineStartData gearStart = campaignGearStartData ?? new GearEngineStartData();

            Track = new TrackViewModel(trackService.CurrentSession);
            BindChildViewModel(Track);

            Board = new BoardViewModel(engineService, gridManager, nodeFactory, boardConfig, presentationTransferService, eventBus, featureToggle, dragService, swapService, mergeService, initialLayout: gearStart.BoardLayout);
            BindChildViewModel(Board);

            Inventory = new GearInventoryViewModel(gearStart.MaxInventorySlots, gearStart.InventoryGears, engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        public void GoToRace()
        {
            try
            {
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
    }
}
