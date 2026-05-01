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

namespace GearEngine.Campaign.Presentation
{
    /// <summary>
    /// ViewModel for the TalentPerks screen.
    /// On initialize it fetches the player's owned perks from the backend and builds one
    /// <see cref="PerkItemViewModel"/> per distinct perk type. Supports buying a random perk
    /// (duplicates allowed) and burning any owned perk copy for gold.
    /// </summary>
    public sealed partial class TalentPerksViewModel : ViewModel, IDisposable
    {
        public IReadOnlyList<ItemPerkViewModel> Items => items;

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

        private readonly List<ItemPerkViewModel> items = new List<ItemPerkViewModel>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private bool disposed;

        protected override void Initialize()
        {
            base.Initialize();
            _ = LoadPerksAsync(cts.Token);
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

        /// <summary>
        /// Purchases a random perk from the backend (duplicates allowed) and adds it to the list.
        /// Called by <see cref="TalentPerksView"/> when the Buy button is clicked.
        /// </summary>
        public async void BuyRandom()
        {
            if (IsBuying)
            {
                return;
            }

            try
            {
                IsBuying = true;
                PurchasePerkResponse response = await perksClient.PurchaseAsync(cts.Token);
                if (response == null)
                {
                    return;
                }

                if (!response.Success || string.IsNullOrEmpty(response.UnlockedPerkId))
                {
                    Debug.LogWarning($"[TalentPerksViewModel] BuyRandom: purchase not successful (Success={response.Success}).");
                    return;
                }

                if (currencyClient != null)
                {
                    long oldGold = currencyClient.GetWallet("gold")?.Current ?? 0;
                    long newGold = Math.Max(0, oldGold - response.Cost);
                    currencyClient.SyncWalletBalance("gold", newGold);
                }

                ItemPerkViewModel newItem = AddOrIncrementItem(response.UnlockedPerkId);
                ItemsRevision++;
                OnPropertyChanged(nameof(NextCost));
                OnPropertyChanged(nameof(CurrentGold));
                OnPropertyChanged(nameof(CanAffordBuy));

                Debug.Log($"[TalentPerksViewModel] Comprado perk: {response.UnlockedPerkId}. Ouro atual: {CurrentGold}. Próximo custo: {NextCost}");

                // Display the newly bought perk in the UI
                if (newItem != null)
                {
                    OpenPerkPopup(newItem);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TalentPerksViewModel] BuyRandom failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsBuying = false;
            }
        }

        /// <summary>
        /// Burns one copy of the given perk item, removing it from the list if the count reaches zero.
        /// </summary>
        public async Task<bool> BurnPerk(string perkId)
        {
            if (string.IsNullOrEmpty(perkId))
            {
                return false;
            }

            try
            {
                BurnPerkResponse response = await perksClient.BurnAsync(perkId, cts.Token);
                if (response == null || !response.Success)
                {
                    Debug.LogWarning($"[TalentPerksViewModel] BurnPerk '{perkId}' failed on server.");
                    return false;
                }

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
                Debug.LogError($"[TalentPerksViewModel] BurnPerk failed: {ex.Message}\n{ex.StackTrace}");
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
                Debug.LogError($"[TalentPerksViewModel] CloseMenu failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task LoadPerksAsync(CancellationToken ct)
        {
            try
            {
                await perksClient.InitializeAsync(ct);
                RebuildItemsFromOwned();
                ItemsRevision++;
                OnPropertyChanged(nameof(NextCost));
                OnPropertyChanged(nameof(CurrentGold));
                OnPropertyChanged(nameof(CanAffordBuy));
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation on dispose – silently ignored.
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TalentPerksViewModel] LoadPerksAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RebuildItemsFromOwned()
        {
            items.Clear();

            IReadOnlyList<string> owned = perksClient.Unlocked;
            if (owned == null)
            {
                return;
            }

            var counts = new Dictionary<string, int>();
            foreach (string id in owned)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (!counts.ContainsKey(id)) counts[id] = 0;
                    counts[id]++;
                }
            }

            foreach (var kvp in counts)
            {
                CreateItemViewModel(kvp.Key, kvp.Value);
            }
        }

        private ItemPerkViewModel AddOrIncrementItem(string perkId)
        {
            RebuildItemsFromOwned();
            
            // Return the created or updated item
            foreach (var item in items)
            {
                if (item.Item.Id == perkId)
                    return item;
            }
            return null;
        }

        private ItemPerkViewModel CreateItemViewModel(string perkId, int amount = 1)
        {
            var config = perkCatalog.Get(perkId);
            if (config != null)
            {
                ItemPerkViewModel vm = new ItemPerkViewModel(config.Data, OpenPerkPopup, amount);
                BindChildViewModel(vm);
                items.Add(vm);
                return vm;
            }
            else
            {
                Debug.LogError($"[TalentPerksViewModel] Perk config not found for '{perkId}' in perkCatalog!");
            }
            return null;
        }

        private void OpenPerkPopup(ItemPerkViewModel item)
        {
            try
            {
                int index = items.IndexOf(item);
                if (index >= 0)
                {
                    navigation.Open(new PerkPopupViewModel(items, index, BurnPerkPopupHandler));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TalentPerksViewModel] OpenPerkPopup failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task<bool> BurnPerkPopupHandler(string perkId)
        {
            bool success = await BurnPerk(perkId);
            if (success)
            {
                Debug.LogWarning("TODO: Needs visual feedback for burning perk");
            }
            return success;
        }
    }
}
