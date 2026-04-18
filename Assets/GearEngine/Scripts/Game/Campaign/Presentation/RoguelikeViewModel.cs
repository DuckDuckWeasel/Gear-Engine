using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.Campaign.Services;
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

                inventoryService.AddItem(selectedCard.GearConfig.CreateRuntimeData());
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] Confirm failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SetupGearEngineSubtree()
        {
            Board = new BoardViewModel(engineService, gridManager, nodeFactory, boardConfig, presentationTransferService, eventBus, featureToggle, dragService, swapService, mergeService, initialLayout: null);
            BindChildViewModel(Board);
            Inventory = new GearInventoryViewModel(maxInventorySlots: inventoryService.MaxSlots, inventoryGears: null, engineService, inventoryService, dragService);
            BindChildViewModel(Inventory);
            TrashZone = new TrashZoneViewModel(dragService, engineService, Board, presentationTransferService, featureToggle);
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
