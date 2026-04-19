using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
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
    public sealed partial class RoguelikeViewModel : ViewModel
    {
        public BoardViewModel Board { get; private set; }
        public GearInventoryViewModel Inventory { get; private set; }
        public TrashZoneViewModel TrashZone { get; private set; }
        public IReadOnlyList<CardOptionViewModel> CardOptions => cardOptions;

        internal IDragService DragService => dragService;

        [ObservableProperty] private bool canConfirm;

        private readonly List<CardOptionViewModel> cardOptions = new List<CardOptionViewModel>();
        private CardOptionViewModel selectedCard;

        [Inject] private ITrackService trackService;
        [Inject] private IGearEngineService engineService;
        [Inject] private IBoardService boardService;
        [Inject] private GearEngineFeatureToggleSO featureToggle;
        [Inject] private IDragService dragService;
        [Inject] private IInventoryService inventoryService;
        [Inject] private IGearPresentationTransferService presentationTransferService;

        protected override void Initialize()
        {
            base.Initialize();
            SetupGearEngineSubtree();
            AddRoguelikeCardsFromTrackService();
        }

        public void SelectCard(CardOptionViewModel card)
        {
            if (card == null)
            {
                return;
            }

            selectedCard?.Deselect();
            selectedCard = card;
            selectedCard.Select();
            CanConfirm = true;
        }

        public void Confirm()
        {
            try
            {
                if (selectedCard == null)
                {
                    throw new InvalidOperationException("[RoguelikeViewModel] No card selected.");
                }

                inventoryService.TryAdd(selectedCard.GearConfig.CreateRuntimeData());
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] Confirm failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SetupGearEngineSubtree()
        {
            Board = new BoardViewModel(boardService, inventoryService, engineService);
            BindChildViewModel(Board);
            Inventory = new GearInventoryViewModel(engineService, inventoryService);
            BindChildViewModel(Inventory);
            TrashZone = new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        private void AddRoguelikeCardsFromTrackService()
        {
            foreach (GearConfig config in trackService.GetRoguelikeCardOptions())
            {
                if (config == null)
                {
                    continue;
                }

                CardOptionViewModel card = new CardOptionViewModel(config);
                BindChildViewModel(card);
                cardOptions.Add(card);
            }
        }
    }
}
