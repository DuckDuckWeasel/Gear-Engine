using System.Threading;
using System.Threading.Tasks;
using Scaffold.Ads;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using Unity.Services.Authentication;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class AdsClientModule : IAsyncInitializable
    {
        public AdsClientModule(AdManager adManager, ILiveOpsService liveOpsService)
        {
            this.adManager = adManager;
            this.liveOpsService = liveOpsService;
        }

        private readonly AdManager adManager;
        private readonly ILiveOpsService liveOpsService;

        public async Task InitializeAsync(CancellationToken ct)
        {
            string userId = AuthenticationService.Instance.PlayerId;
            LiveOpsRewardEndpointClient rewardClient = new LiveOpsRewardEndpointClient(liveOpsService);
            
            await adManager.InitializeAds(userId, rewardClient);
            
            await Task.CompletedTask;
        }
    }
}
