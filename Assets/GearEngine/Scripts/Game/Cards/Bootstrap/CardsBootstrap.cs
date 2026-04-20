using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap.Cards;
using GearEngine.Currency;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Cards.Bootstrap
{
    public sealed class CardsBootstrap : MonoBehaviour, IInitializable
    {
        [Inject]
        private CardCatalogSO catalog;

        [Inject]
        private INavigation navigation;

        [Inject]
        private IObjectResolver resolver;

        public void Initialize()
        {
            try
            {
                if (catalog == null)
                {
                    throw new InvalidOperationException("[CardsBootstrap] CardCatalogSO is missing.");
                }

                StartCoroutine(WarmupLiveOpsThenOpen());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardsBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private IEnumerator WarmupLiveOpsThenOpen()
        {
            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            CardsClientModule cards = resolver.Resolve<CardsClientModule>();
            Task warmup = Task.WhenAll(
                currency.InitializeAsync(CancellationToken.None),
                cards.InitializeAsync(CancellationToken.None));
            yield return new WaitUntil(() => warmup.IsCompleted);
            if (warmup.IsFaulted && warmup.Exception != null)
            {
                Debug.LogError($"[CardsBootstrap] LiveOps warmup failed: {warmup.Exception.GetBaseException().Message}\n{warmup.Exception}");
                yield break;
            }

            try
            {
                CardSampleViewModel viewModel = resolver.Resolve<CardSampleViewModel>();
                viewModel.RefreshDisplay();
                navigation.Open(viewModel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardsBootstrap] Open CardSampleViewModel failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
