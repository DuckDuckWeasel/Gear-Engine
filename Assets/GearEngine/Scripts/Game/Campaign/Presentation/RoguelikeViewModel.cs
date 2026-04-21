using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class RoguelikeViewModel : ViewModel, IDisposable
    {
        public BoardViewModel Board { get; private set; }
        public GearInventoryViewModel Inventory { get; private set; }
        public TrashZoneViewModel TrashZone { get; private set; }
        public IReadOnlyList<CardOptionViewModel> CardOptions => cardOptions;

        internal IDragService DragService => dragService;

        [ObservableProperty]
        private bool canConfirm;

        [ObservableProperty]
        private int cardOptionsRevision;

        private readonly List<CardOptionViewModel> cardOptions = new List<CardOptionViewModel>();
        private CardOptionViewModel selectedCard;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private bool disposed;

        [Inject]
        private IRoguelikeRollService rollService;

        [Inject]
        private IGearEngineService engineService;

        [Inject]
        private IBoardService boardService;

        [Inject]
        private GearEngineFeatureToggleSO featureToggle;

        [Inject]
        private IDragService dragService;

        [Inject]
        private IInventoryService inventoryService;

        [Inject]
        private IGearPresentationTransferService presentationTransferService;

        protected override void Initialize()
        {
            base.Initialize();
            SetupGearEngineSubtree();
            _ = LoadRollAsync(cts.Token);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            cts.Cancel();
            cts.Dispose();
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
            RecomputeCanConfirm();
        }

        public async void Confirm()
        {
            try
            {
                await ConfirmPickAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] Confirm failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SetupGearEngineSubtree()
        {
            Board = new BoardViewModel(boardService, engineService, inventoryService);
            BindChildViewModel(Board);
            Inventory = new GearInventoryViewModel(engineService, boardService, inventoryService);
            BindChildViewModel(Inventory);
            TrashZone = new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        private async Task LoadRollAsync(CancellationToken ct)
        {
            try
            {
                await AppendRollOptionsAsync(ct);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] LoadRollAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task ConfirmPickAsync()
        {
            if (selectedCard == null)
            {
                throw new InvalidOperationException("[RoguelikeViewModel] No card selected.");
            }

            if (inventoryService.Add(selectedCard.GearConfig) == null)
            {
                return;
            }

            await rollService.ConsumePickAsync(selectedCard.GearConfig, cts.Token);
            navigation.Open(new MainViewModel());
        }

        private async Task AppendRollOptionsAsync(CancellationToken ct)
        {
            IReadOnlyList<GearConfig> options = await rollService.GetCurrentRollAsync(ct);
            foreach (GearConfig config in options)
            {
                AddCardOption(config);
            }

            CardOptionsRevision++;
        }

        private void AddCardOption(GearConfig config)
        {
            if (config == null)
            {
                return;
            }

            CardOptionViewModel card = new CardOptionViewModel(config);
            BindChildViewModel(card);
            cardOptions.Add(card);
        }

        private void RecomputeCanConfirm()
        {
            CanConfirm = selectedCard != null;
        }
    }
}
