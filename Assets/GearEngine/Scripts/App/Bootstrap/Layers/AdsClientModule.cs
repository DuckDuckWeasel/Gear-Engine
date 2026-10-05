using System;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.Ads;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using Unity.Services.Authentication;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class AdsClientModule : IAsyncInitializable
    {
        public AdsClientModule(
            AdManager adManager,
            ILiveOpsService liveOpsService,
            OfflineSessionState offlineSessionState)
        {
            this.adManager = adManager ?? throw new ArgumentNullException(nameof(adManager));
            this.liveOpsService = liveOpsService ?? throw new ArgumentNullException(nameof(liveOpsService));
            this.offlineSessionState = offlineSessionState ?? throw new ArgumentNullException(nameof(offlineSessionState));
        }

        private readonly AdManager adManager;
        private readonly ILiveOpsService liveOpsService;
        private readonly OfflineSessionState offlineSessionState;

        public async Task InitializeAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                if (offlineSessionState.IsOffline
                    || Application.internetReachability == NetworkReachability.NotReachable)
                {
                    return;
                }

                string userId = AuthenticationService.Instance.IsSignedIn
                    ? AuthenticationService.Instance.PlayerId
                    : "offline_player";
                LiveOpsRewardEndpointClient rewardClient = new LiveOpsRewardEndpointClient(liveOpsService);
                await adManager.InitializeAds(userId, rewardClient);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[AdsClientModule] Ads are unavailable; continuing without ads. {exception}");
            }
        }
    }
}
