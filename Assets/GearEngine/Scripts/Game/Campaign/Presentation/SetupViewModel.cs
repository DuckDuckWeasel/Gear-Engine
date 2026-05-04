using System;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Nodes;
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
        [Inject] private IInventoryService inventoryService;
        [Inject] private IGearPresentationTransferService presentationTransferService;
        [Inject] private IGearLoadoutService loadoutService;

        protected override void Initialize()
        {
            base.Initialize();

            engineService.ResetGridSimulationState();

            Track = new CampaignTrackPreviewViewModel(trackService.CurrentTrack);
            BindChildViewModel(Track);

            Board = new BoardViewModel(boardService, engineService, inventoryService);
            BindChildViewModel(Board);

            BoardLayoutData savedLayout = loadoutService.GetBoardLayout();
            if (savedLayout != null)
            {
                Board.LoadLayout(savedLayout);
            }

            Inventory = new GearInventoryViewModel(engineService, boardService, inventoryService);
            BindChildViewModel(Inventory);

            TrashZone = new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);

            Board.OnBoardClicked += ShowItemPreview;
            Inventory.OnInventoryClicked += ShowItemPreview;
        }

        protected override void OnClosed()
        {
            if (Board != null)
            {
                Board.OnBoardClicked -= ShowItemPreview;
            }
            if (Inventory != null)
            {
                Inventory.OnInventoryClicked -= ShowItemPreview;
            }
            base.OnClosed();
        }

        private void ShowItemPreview(IGridNode node)
        {
            if (node != null && node.ConfigData != null)
            {
                ShowItemPreview(node.ConfigData);
            }
        }

        private void ShowItemPreview(GearItemData gearData)
        {
            if (gearData == null) return;
            
            ItemSlotViewModel tempSlot = new ItemSlotViewModel(gearData, _ => { }, 1);
            ItemPopupViewModel popup = new ItemPopupViewModel(new[] { tempSlot }, 0, null);
            navigation.Open(popup);
        }

        public void GoToRace()
        {
            try
            {
                if (!boardService.ContainsMotorCog)
                {
                    Debug.LogError("[SetupViewModel] Cannot start race: motor cog missing from loadout.");
                    return;
                }

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
