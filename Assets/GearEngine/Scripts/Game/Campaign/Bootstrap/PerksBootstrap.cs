using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.Perks;
using GearEngine.Currency;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using GearEngine.Perks.Config;

namespace GearEngine.Perks.Bootstrap
{
    public sealed class PerksBootstrap : MonoBehaviour, IInitializable
    {
        [Inject]
        private PerkCatalogSO catalog;

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
                    throw new InvalidOperationException("[PerksBootstrap] PerkCatalogSO is missing.");
                }

                StartCoroutine(WarmupLiveOpsThenOpen());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerksBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private IEnumerator WarmupLiveOpsThenOpen()
        {
            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            PerksClientModule perks = resolver.Resolve<PerksClientModule>();
            Task warmup = Task.WhenAll(
                currency.InitializeAsync(CancellationToken.None),
                perks.InitializeAsync(CancellationToken.None));
            yield return new WaitUntil(() => warmup.IsCompleted);
            if (warmup.IsFaulted && warmup.Exception != null)
            {
                Debug.LogError($"[PerksBootstrap] LiveOps warmup failed: {warmup.Exception.GetBaseException().Message}\n{warmup.Exception}");
                yield break;
            }

            try
            {
                PerkSampleViewModel viewModel = resolver.Resolve<PerkSampleViewModel>();
                viewModel.RefreshDisplay();
                navigation.Open(viewModel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PerksBootstrap] Open PerkSampleViewModel failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
