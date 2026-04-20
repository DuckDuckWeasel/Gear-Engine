using System;
using System.Linq;
using GearEngine.Campaign.Services;
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
        public CampaignTrackPreviewViewModel Track { get; private set; }
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
        [Inject] private IRaceInventoryService inventoryService;
        [Inject] private IGearPresentationTransferService presentationTransferService;
        [Inject] private IGearLoadoutService loadoutService;

        protected override void Initialize()
        {
            base.Initialize();

            engineService.ResetGridSimulationState();

            Track = new CampaignTrackPreviewViewModel(trackService.CurrentTrack);
            BindChildViewModel(Track);

            Board = new BoardViewModel(boardService, inventoryService, engineService);
            BindChildViewModel(Board);

            if (loadoutService.HasSavedLoadout && !boardService.GetAllNodes().Any())
            {
                Board.LoadLayout(loadoutService.GetBoardLayout());
            }

            Inventory = new GearInventoryViewModel(engineService, inventoryService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        public void GoToRace()
        {
            try
            {
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
    }
}
