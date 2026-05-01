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
        private const int MaxInventoryCapacity = 10; // TODO: Move to config

        public BoardViewModel Board { get; private set; }
        public GearInventoryViewModel Inventory { get; private set; }
        public TrashZoneViewModel TrashZone { get; private set; }
        public IReadOnlyList<CardOptionViewModel> CardOptions => cardOptions;

        internal IDragService DragService => dragService;

        [ObservableProperty]
        private int cardOptionsRevision;

        [ObservableProperty]
        private bool canReroll = true;

        private readonly List<CardOptionViewModel> cardOptions = new List<CardOptionViewModel>();
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
            CanReroll = true;
            SetupGearEngineSubtree();
            inventoryService.InventoryChanged += UpdateCardOptionsInteractability;
            _ = LoadRollAsync(cts.Token);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            inventoryService.InventoryChanged -= UpdateCardOptionsInteractability;
            cts.Cancel();
            cts.Dispose();
        }

        private void UpdateCardOptionsInteractability()
        {
            bool hasSpace = inventoryService.Owned.Count < MaxInventoryCapacity;
            foreach (CardOptionViewModel card in cardOptions)
            {
                card.CanPick = hasSpace;
            }
        }

        public async void PickCard(CardOptionViewModel card)
        {
            if (card == null)
            {
                return;
            }

            if (inventoryService.Owned.Count >= MaxInventoryCapacity)
            {
                Debug.LogWarning("TODO: Show visual warning - Inventory Full");
                return;
            }

            try
            {
                if (inventoryService.Add(card.Config) == null)
                {
                    return;
                }

                await rollService.ConsumePickAsync(card.Config, cts.Token);
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] PickCard failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async void Continue()
        {
            try
            {
                await SkipPickAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async void Reroll()
        {
            if (!CanReroll) return;

            try
            {
                CanReroll = false;
                Debug.LogWarning("TODO: Trigger ad");
                await ReRerollAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] Reroll failed: {ex.Message}\n{ex.StackTrace}");
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



        private async Task SkipPickAsync()
        {
            await rollService.SkipPickAsync(cts.Token);
            navigation.Open(new MainViewModel());
        }
        
        private async Task ReRerollAsync()
        {
            cardOptions.Clear();

            IReadOnlyList<GearConfig> options = await rollService.RerollAsync(cts.Token);
            foreach (GearConfig config in options)
            {
                AddCardOption(config);
            }

            CardOptionsRevision++;
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

            CardOptionViewModel card = new CardOptionViewModel(config, PickCard);
            card.CanPick = inventoryService.Owned.Count < MaxInventoryCapacity;
            BindChildViewModel(card);
            cardOptions.Add(card);
        }
    }
}
