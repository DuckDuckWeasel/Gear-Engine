using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Presentation;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap
{
    public sealed class CampaignBootstrap : MonoBehaviour, IInitializable
    {
        [Inject] private INavigation navigation;
        [Inject] private TracksClientModule tracksClient;
        [Inject] private LoadoutClientModule loadoutClient;
        [Inject] private InventoryClientModule inventoryClient;
        [Inject] private CardsClientModule cardsClient;

        public void Initialize()
        {
            try
            {
                StartCoroutine(WarmupLiveOpsClientsThenOpenMain());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CampaignBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private IEnumerator WarmupLiveOpsClientsThenOpenMain()
        {
            Task t = WarmupAllClientsAsync();
            yield return new WaitUntil(() => t.IsCompleted);
            if (t.IsFaulted && t.Exception != null)
            {
                Debug.LogError($"[CampaignBootstrap] LiveOps client warmup failed: {t.Exception.GetBaseException().Message}\n{t.Exception}");
                yield break;
            }

            try
            {
                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CampaignBootstrap] Open MainViewModel failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task WarmupAllClientsAsync()
        {
            CancellationToken ct = CancellationToken.None;
            await tracksClient.InitializeAsync(ct).ConfigureAwait(true);
            await loadoutClient.InitializeAsync(ct).ConfigureAwait(true);
            await inventoryClient.InitializeAsync(ct).ConfigureAwait(true);
            await cardsClient.InitializeAsync(ct).ConfigureAwait(true);
        }
    }
}
