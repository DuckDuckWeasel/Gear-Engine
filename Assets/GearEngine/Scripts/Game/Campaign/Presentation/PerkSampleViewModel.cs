using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveOps.Modules.DTO.ModuleRequests;
using PurchasePerkResponse = LiveOps.Modules.DTO.ModuleRequests.PurchasePerkResponse;
using BurnPerkResponse = LiveOps.Modules.DTO.ModuleRequests.BurnPerkResponse;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Currency;
using Scaffold.MVVM;
using UnityEngine;
using GearEngine.Perks.Config;

namespace GearEngine.Perks
{
    public sealed partial class PerkSampleViewModel : ViewModel
    {
        public PerkSampleViewModel(PerkCatalogSO catalog, CurrencyClientModule currencyClient, PerksClientModule perksClient)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.currencyClient = currencyClient ?? throw new ArgumentNullException(nameof(currencyClient));
            this.perksClient = perksClient ?? throw new ArgumentNullException(nameof(perksClient));
        }

        private readonly PerkCatalogSO catalog;
        private readonly CurrencyClientModule currencyClient;
        private readonly PerksClientModule perksClient;

        [ObservableProperty]
        private long gold;

        [ObservableProperty]
        private long nextCost;

        [ObservableProperty]
        private int perksRevision;

        public string CurrencyId => perksClient.CurrencyId;

        public IReadOnlyList<string> UnlockedPerkIds => perksClient.Unlocked ?? Array.Empty<string>();

        /// <summary>Call after LiveOps client modules have completed <see cref="Scaffold.AppFlow.IAsyncInitializable.InitializeAsync"/>.</summary>
        public void RefreshDisplay()
        {
            Gold = currencyClient.GetWallet("gold")?.Current ?? 0;
            NextCost = perksClient.NextCost;
            PerksRevision++;
        }

        public string GetDisplayLabelForPerk(string perkId)
        {
            if (string.IsNullOrEmpty(perkId) || catalog == null)
            {
                return perkId ?? string.Empty;
            }

            return catalog.TryGet(perkId, out PerkItem def) && def != null && !string.IsNullOrEmpty(def.Id)
                ? def.Id
                : perkId;
        }

        public async void TryPurchaseRandomPerk()
        {
            try
            {
                PurchasePerkResponse response = await perksClient.PurchaseAsync();
                if (response == null)
                {
                    return;
                }

                if (!response.Success && !string.IsNullOrEmpty(response.UnlockedPerkId))
                {
                    Debug.LogWarning("[PerkSampleViewModel] Purchase reported failure but returned a perk id.");
                }

                RefreshDisplay();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerkSampleViewModel] TryPurchaseRandomPerk failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
