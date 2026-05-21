using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using GearEngine.Perks.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class ItemsViewModel : ViewModel, IDisposable
    {
        public ItemsScreenState Config { get; }

        public IReadOnlyList<ItemSlotViewModel> Items => items;

        public long NextCost => perksClient?.NextCost ?? 0;
        public long CurrentGold => currencyClient?.GetWallet("gold")?.Current ?? 0;
        public bool CanAffordBuy => NextCost > 0 && CurrentGold >= NextCost;

        [ObservableProperty]
        private int itemsRevision;

        [ObservableProperty]
        private bool isBuying;

        [Inject]
        private IPerksClientModule perksClient;

        [Inject]
        private CurrencyClientModule currencyClient;

        [Inject]
        private global::GearEngine.Perks.Config.PerkCatalogSO perkCatalog;

        [Inject]
        private IInventoryService inventoryClient;

        [Inject]
        private GearCatalogSO gearCatalog;

        private readonly List<ItemSlotViewModel> items = new List<ItemSlotViewModel>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private bool disposed;

        public ItemsViewModel(ItemsScreenState config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        protected override void Initialize()
        {
            base.Initialize();
            _ = LoadItemsAsync(cts.Token);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            cts.Cancel();
            cts.Dispose();
        }

        public async void BuyRandom()
        {
            if (IsBuying || Config.TypeToDisplay != ItemScreenType.Perks) return;

            try
            {
                IsBuying = true;
                PurchasePerkResponse response = await perksClient.PurchaseAsync(cts.Token);
                if (response == null || !response.Success || string.IsNullOrEmpty(response.UnlockedPerkId))
                {
                    return;
                }

                if (currencyClient != null)
                {
                    long oldGold = currencyClient.GetWallet("gold")?.Current ?? 0;
                    long newGold = Math.Max(0, oldGold - response.Cost);
                    currencyClient.SyncWalletBalance("gold", newGold);
                }

                ItemSlotViewModel newItem = AddOrIncrementItem(response.UnlockedPerkId);
                ItemsRevision++;
                OnPropertyChanged(nameof(NextCost));
                OnPropertyChanged(nameof(CurrentGold));
                OnPropertyChanged(nameof(CanAffordBuy));

                if (newItem != null)
                {
                    OpenItemPopup(newItem);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsViewModel] BuyRandom failed: {ex.Message}");
            }
            finally
            {
                IsBuying = false;
            }
        }

        public async Task<bool> BurnItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || Config.TypeToDisplay != ItemScreenType.Perks) return false;

            try
            {
                BurnPerkResponse response = await perksClient.BurnAsync(itemId, cts.Token);
                if (response == null || !response.Success) return false;

                if (currencyClient != null)
                {
                    currencyClient.SyncWalletBalance("gold", response.NewGoldBalance);
                }

                RebuildItemsFromOwned();
                ItemsRevision++;
                OnPropertyChanged(nameof(NextCost));
                OnPropertyChanged(nameof(CurrentGold));
                OnPropertyChanged(nameof(CanAffordBuy));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsViewModel] BurnItem failed: {ex.Message}");
                return false;
            }
        }

        public void CloseMenu()
        {
            try
            {
                navigation.Open(new MainViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsViewModel] CloseMenu failed: {ex.Message}");
            }
        }

        private async Task LoadItemsAsync(CancellationToken ct)
        {
            try
            {
                if (Config.TypeToDisplay == ItemScreenType.Perks && perksClient != null)
                {
                    await perksClient.InitializeAsync(ct);
                }
                
                RebuildItemsFromOwned();
                ItemsRevision++;
                OnPropertyChanged(nameof(NextCost));
                OnPropertyChanged(nameof(CurrentGold));
                OnPropertyChanged(nameof(CanAffordBuy));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsViewModel] LoadItemsAsync failed: {ex.Message}");
            }
        }

        private void RebuildItemsFromOwned()
        {
            items.Clear();
            var counts = new Dictionary<string, int>();

            if (Config.TypeToDisplay == ItemScreenType.Perks)
            {
                if (perksClient?.Unlocked != null)
                {
                    foreach (string id in perksClient.Unlocked)
                    {
                        if (string.IsNullOrEmpty(id)) continue;
                        if (!counts.ContainsKey(id)) counts[id] = 0;
                        counts[id]++;
                    }
                }

                foreach (var perk in perkCatalog.All)
                {
                    if (perk != null && perk.Data != null)
                    {
                        counts.TryGetValue(perk.Id, out int amount);
                        if (!Config.ShowUnownedItems && amount <= 0) continue;

                        ItemSlotViewModel vm = new ItemSlotViewModel(perk.Data, OpenItemPopup, amount);
                        BindChildViewModel(vm);
                        items.Add(vm);
                    }
                }
            }
            else if (Config.TypeToDisplay == ItemScreenType.Gears)
            {
                if (inventoryClient?.Owned != null)
                {
                    foreach (var gear in inventoryClient.Owned)
                    {
                        if (gear?.Config == null || string.IsNullOrEmpty(gear.Config.Id)) continue;
                        if (!counts.ContainsKey(gear.Config.Id)) counts[gear.Config.Id] = 0;
                        counts[gear.Config.Id]++;
                    }
                }

                foreach (var gearItem in gearCatalog.All)
                {
                    if (gearItem != null)
                    {
                        counts.TryGetValue(gearItem.Id, out int amount);
                        if (!Config.ShowUnownedItems && amount <= 0) continue;

                        IItem iitem = gearItem.CreateRuntimeData();
                        ItemSlotViewModel vm = new ItemSlotViewModel(iitem, OpenItemPopup, amount);
                        BindChildViewModel(vm);
                        items.Add(vm);
                    }
                }
            }
        }

        private ItemSlotViewModel AddOrIncrementItem(string itemId)
        {
            RebuildItemsFromOwned();
            foreach (var item in items)
            {
                if (item.Item.Id == itemId) return item;
            }
            return null;
        }

        private void OpenItemPopup(ItemSlotViewModel item)
        {
            try
            {
                int index = items.IndexOf(item);
                if (index >= 0)
                {
                    if (Config.TypeToDisplay == ItemScreenType.Perks)
                    {
                        navigation.Open(new ItemPopupViewModel(items, index, BurnItemPopupHandler));
                    }
                    else if (Config.TypeToDisplay == ItemScreenType.Gears)
                    {
                        // Gears don't support burning yet, pass null or a dummy handler
                        navigation.Open(new ItemPopupViewModel(items, index, null));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemsViewModel] OpenItemPopup failed: {ex.Message}");
            }
        }

        private async Task<bool> BurnItemPopupHandler(string itemId)
        {
            bool success = await BurnItem(itemId);
            if (success)
            {
                // visual feedback
            }
            return success;
        }
    }
}
