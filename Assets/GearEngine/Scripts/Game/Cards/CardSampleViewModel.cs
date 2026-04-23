using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Currency;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.Cards
{
    public sealed partial class CardSampleViewModel : ViewModel
    {
        public CardSampleViewModel(CardCatalogSO catalog, CurrencyClientModule currencyClient, CardsClientModule cardsClient)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.currencyClient = currencyClient ?? throw new ArgumentNullException(nameof(currencyClient));
            this.cardsClient = cardsClient ?? throw new ArgumentNullException(nameof(cardsClient));
        }

        private readonly CardCatalogSO catalog;
        private readonly CurrencyClientModule currencyClient;
        private readonly CardsClientModule cardsClient;

        [ObservableProperty]
        private long gold;

        [ObservableProperty]
        private long nextCost;

        [ObservableProperty]
        private int cardsRevision;

        public string CurrencyId => cardsClient.CurrencyId;

        public IReadOnlyList<string> UnlockedCardIds => cardsClient.Unlocked ?? Array.Empty<string>();

        /// <summary>Call after LiveOps client modules have completed <see cref="Scaffold.AppFlow.IAsyncInitializable.InitializeAsync"/>.</summary>
        public void RefreshDisplay()
        {
            Gold = currencyClient.GetWallet("gold")?.Current ?? 0;
            NextCost = cardsClient.NextCost;
            CardsRevision++;
        }

        public string GetDisplayLabelForCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || catalog == null)
            {
                return cardId ?? string.Empty;
            }

            return catalog.TryGet(cardId, out CardDefinition def) && def != null && !string.IsNullOrEmpty(def.Id)
                ? def.Id
                : cardId;
        }

        public async void TryPurchaseRandomCard()
        {
            try
            {
                PurchaseCardResponse response = await cardsClient.PurchaseAsync();
                if (response == null)
                {
                    return;
                }

                if (!response.Success && !string.IsNullOrEmpty(response.UnlockedCardId))
                {
                    Debug.LogWarning("[CardSampleViewModel] Purchase reported failure but returned a card id.");
                }

                RefreshDisplay();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardSampleViewModel] TryPurchaseRandomCard failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
