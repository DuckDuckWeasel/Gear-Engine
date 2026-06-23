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
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Nodes;
using Scaffold.Ads;
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
        public IReadOnlyList<ItemSlotViewModel> PerkOptions => perkOptions;

        internal IDragService DragService => dragService;

        [ObservableProperty]
        private int perkOptionsRevision;

        [ObservableProperty]
        private bool canReroll = true;

        private readonly List<ItemSlotViewModel> perkOptions = new List<ItemSlotViewModel>();
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

        [Inject]
        private RewardedAdManager adManager;

        [Inject]
        private AdPlacementKeySO rerollPlacementKey;

        [Inject]
        private ToolbarController toolbarController;

        [ObservableProperty]
        private bool isProcessingAction;

        private bool hasRerolled;

        protected override void Initialize()
        {
            base.Initialize();
            IsProcessingAction = false;
            hasRerolled = false;
            CanReroll = false;
            
            adManager.AdAvailable += OnAdAvailable;
            _ = CheckInitialAdStateAsync();

            SetupGearEngineSubtree();
            inventoryService.InventoryChanged += UpdatePerkOptionsInteractability;
            _ = LoadRollAsync(cts.Token);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            inventoryService.InventoryChanged -= UpdatePerkOptionsInteractability;
            adManager.AdAvailable -= OnAdAvailable;
            
            if (Board != null)
            {
                Board.OnBoardClicked -= ShowItemPreview;
            }
            if (Inventory != null)
            {
                Inventory.OnInventoryClicked -= ShowItemPreview;
            }

            cts.Cancel();
            cts.Dispose();
        }

        private void OnAdAvailable(bool available)
        {
            if (!IsProcessingAction && !hasRerolled)
            {
                CanReroll = available;
            }
        }

        private async Task CheckInitialAdStateAsync()
        {
            try
            {
                string placementId = rerollPlacementKey != null ? (string)rerollPlacementKey : "reroll";
                bool available = await adManager.CanShowAd(placementId);
                OnAdAvailable(available);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeViewModel] CheckInitialAdStateAsync failed: {ex.Message}");
            }
        }

        private void UpdatePerkOptionsInteractability()
        {
            bool hasSpace = inventoryService.Owned.Count < MaxInventoryCapacity;
            foreach (ItemSlotViewModel perk in perkOptions)
            {
                perk.CanPick = hasSpace;
            }
        }

        private async Task PickPerkAsync(ItemSlotViewModel perk)
        {
            Debug.Log($"[RoguelikeViewModel] PickPerkAsync called for perk: {perk?.Item?.Id}");
            if (IsProcessingAction || perk == null)
            {
                Debug.LogWarning($"[RoguelikeViewModel] PickPerkAsync aborted. isProcessingAction: {IsProcessingAction}, perk is null: {perk == null}");
                return;
            }

            if (inventoryService.Owned.Count >= MaxInventoryCapacity)
            {
                Debug.LogWarning("TODO: Show visual warning - Inventory Full");
                return;
            }

            IsProcessingAction = true;

            try
            {
                GearItemData gearData = (GearItemData)perk.Item;
                GearItem config = gearData.SourceGearConfig;

                Debug.Log($"[RoguelikeViewModel] Adding {config.Id} to inventory.");
                if (inventoryService.Add(config) == null)
                {
                    Debug.LogWarning($"[RoguelikeViewModel] Failed to add {config.Id} to inventory.");
                    isProcessingAction = false;
                    return;
                }

                Debug.Log($"[RoguelikeViewModel] Consuming pick from rollService.");
                await rollService.ConsumePickAsync(config.Id, cts.Token);
                Debug.Log($"[RoguelikeViewModel] Opening MainViewModel.");
                if (toolbarController != null) 
                {
                    toolbarController.OpenMainView();
                }
                else
                {
                    navigation.Open(new MainViewModel(), true, new NavigationOptions { CloseAllViews = true });
                }
            }
            catch (Exception ex)
            {
                IsProcessingAction = false;
                Debug.LogError($"[RoguelikeViewModel] PickPerk failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async void Continue()
        {
            if (IsProcessingAction) return;
            IsProcessingAction = true;

            try
            {
                await SkipPickAsync();
            }
            catch (Exception ex)
            {
                IsProcessingAction = false;
                Debug.LogError($"[RoguelikeViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async void Reroll()
        {
            if (IsProcessingAction || !CanReroll) return;
            IsProcessingAction = true;

            string placementId = rerollPlacementKey != null ? (string)rerollPlacementKey : "reroll";

            try
            {
                bool canShow = await adManager.CanShowAd(placementId);
                if (!canShow)
                {
                    Debug.LogWarning("[RoguelikeViewModel] Ad not available or on cooldown. Reroll aborted.");
                    CanReroll = false;
                    IsProcessingAction = false;
                    return;
                }

                CanReroll = false;
                adManager.AdSuccessfullyCompleted += OnAdCompleted;
                adManager.ClickShowAdReward(placementId);
            }
            catch (Exception ex)
            {
                IsProcessingAction = false;
                Debug.LogError($"[RoguelikeViewModel] Reroll failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void OnAdCompleted(bool success, string placement)
        {
            adManager.AdSuccessfullyCompleted -= OnAdCompleted;
            
            if (success)
            {
                try
                {
                    await ReRerollAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RoguelikeViewModel] ReRerollAsync failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning("[RoguelikeViewModel] Ad failed or was cancelled. Reroll aborted and button removed.");
            }
            
            IsProcessingAction = false;
        }

        private void SetupGearEngineSubtree()
        {
            Board = new BoardViewModel(boardService, engineService, inventoryService);
            Board.OnBoardClicked += ShowItemPreview;
            BindChildViewModel(Board);
            Inventory = new GearInventoryViewModel(engineService, boardService, inventoryService);
            Inventory.OnInventoryClicked += ShowItemPreview;
            BindChildViewModel(Inventory);
            TrashZone = new TrashZoneViewModel(engineService, Board, presentationTransferService, featureToggle);
            BindChildViewModel(TrashZone);
        }

        private void ShowItemPreview(IGridNode node)
        {
            Debug.Log($"[RoguelikeViewModel] ShowItemPreview called for node. ConfigData present: {node?.ConfigData != null}");
            if (node != null && node.ConfigData != null)
            {
                ShowItemPreview(node.ConfigData);
            }
        }

        private void ShowItemPreview(GearItemData gearData)
        {
            Debug.Log($"[RoguelikeViewModel] ShowItemPreview called for GearItemData '{gearData?.Id}'. isProcessingAction: {IsProcessingAction}");
            if (IsProcessingAction || gearData == null) return;
            
            ItemSlotViewModel tempSlot = new ItemSlotViewModel(gearData, _ => { }, 1);
            ItemPopupViewModel popup = new ItemPopupViewModel(new[] { tempSlot }, 0, null);
            navigation.Open(popup);
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
            if (toolbarController != null) 
            {
                toolbarController.OpenMainView();
            }
            else
            {
                navigation.Open(new MainViewModel(), true, new NavigationOptions { CloseAllViews = true });
            }
        }
        
        private async Task ReRerollAsync()
        {
            hasRerolled = true;
            CanReroll = false;
            perkOptions.Clear();

            IReadOnlyList<IItem> options = await rollService.RerollAsync(cts.Token);
            foreach (IItem config in options)
            {
                AddPerkOption(config);
            }

            PerkOptionsRevision++;
        }

        private async Task AppendRollOptionsAsync(CancellationToken ct)
        {
            IReadOnlyList<IItem> options = await rollService.GetCurrentRollAsync(ct);
            foreach (IItem config in options)
            {
                AddPerkOption(config);
            }

            PerkOptionsRevision++;
        }

        private void AddPerkOption(IItem config)
        {
            if (config == null)
            {
                return;
            }

            ItemSlotViewModel perk = new ItemSlotViewModel(config, OpenPerkPreview);
            perk.CanPick = inventoryService.Owned.Count < MaxInventoryCapacity;
            BindChildViewModel(perk);
            perkOptions.Add(perk);
        }

        private void OpenPerkPreview(ItemSlotViewModel perk)
        {
            if (IsProcessingAction || perk == null) return;
            
            int index = perkOptions.IndexOf(perk);
            if (index < 0) index = 0;

            ItemPopupViewModel popup = new ItemPopupViewModel(perkOptions, index, ConfirmPickAsync, "Select", false);
            navigation.Open(popup);
        }

        private async Task<bool> ConfirmPickAsync(string itemId)
        {
            Debug.Log($"[RoguelikeViewModel] ConfirmPickAsync called with itemId: {itemId}");
            ItemSlotViewModel perk = null;
            foreach (var p in perkOptions)
            {
                if (p.Item.Id == itemId)
                {
                    perk = p;
                    break;
                }
            }

            if (perk == null) 
            {
                Debug.LogWarning($"[RoguelikeViewModel] ConfirmPickAsync failed: perk '{itemId}' not found in perkOptions.");
                return false;
            }

            Debug.Log($"[RoguelikeViewModel] ConfirmPickAsync found perk, awaiting PickPerkAsync...");
            await PickPerkAsync(perk);
            return true;
        }
    }
}
