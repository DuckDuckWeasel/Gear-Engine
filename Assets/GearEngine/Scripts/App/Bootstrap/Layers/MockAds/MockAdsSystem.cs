using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.Ads;
using Scaffold.AppFlow;
using Unity.Services.Authentication;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Layers.MockAds
{
    public class MockAdConfigurationSO : AdConfigurationSO
    {
        public override IAdProvider CreateProvider() => new MockAdProvider();
        
        public override List<RewardedAdConfig> GetRewardedPlacements() => new List<RewardedAdConfig>();
        public override List<InterstitialAdConfig> GetInterstitialPlacements() => new List<InterstitialAdConfig>();
        public override List<BannerAdConfig> GetBannerPlacements() => new List<BannerAdConfig>();
    }

    public class MockAdProvider : IAdProvider
    {
        public Task Initialize(string userId) => Task.CompletedTask;
        public string UserId { get; set; }
        
        public IRewardedAdService RewardedAdService { get; } = new MockRewardedAdService();
        public IInterstitialAdService InterstitialAdService { get; } = new MockInterstitialAdService();
        public IBannerAdService BannerAdService { get; } = null;
        
        public void SetMuted(bool mute) {}
        public void Dispose() {}
    }

    public class MockRewardedAdService : IRewardedAdService
    {
        public event Action<bool> AdAvailable;
        public event Action<bool, string, string> AdSuccessfullyCompletedWithToken;
        public event Action<bool, string> AdSuccessfullyCompleted;

        public async void Initialize() 
        {
            // Simulate an ad loading after 1 second
            await Awaitable.WaitForSecondsAsync(1f);
            AdAvailable?.Invoke(true);
        }

        public async void ShowAd(string placementName = null) 
        {
            Debug.Log($"<color=yellow>[Mock Ad]</color> Showing mock ad for placement {placementName}. Completing in 2s...");
            // Simulate user watching ad for 2 seconds
            await Awaitable.WaitForSecondsAsync(2f);
            
            Debug.Log($"<color=green>[Mock Ad]</color> Ad finished successfully!");
            // AdSuccessfullyCompletedWithToken?.Invoke(true, placementName, "mock_token_123");
            AdSuccessfullyCompleted?.Invoke(true, placementName);
            
            // Simulate loading the NEXT ad
            await Awaitable.WaitForSecondsAsync(1f);
            AdAvailable?.Invoke(true);
        }

        public async Awaitable<bool> CanShowAd(string placementName = null)
        {
            await Task.Yield();
            return true;
        }
    }

    public class MockInterstitialAdService : IInterstitialAdService
    {
        public event Action<bool> AdAvailable;
        public event Action<bool, string> AdSuccessfullyCompleted;

        public async void ShowAd(string placementName = null) 
        {
            Debug.Log($"<color=yellow>[Mock Ad]</color> Showing mock Interstitial ad for placement {placementName}. Completing in 2s...");
            await Awaitable.WaitForSecondsAsync(2f);
            
            Debug.Log($"<color=green>[Mock Ad]</color> Interstitial Ad finished successfully!");
            AdSuccessfullyCompleted?.Invoke(true, placementName);
        }

        public async Awaitable<bool> CanShowAd(string placementName = null)
        {
            await Task.Yield();
            return true;
        }

        public void Dispose() {}
    }

    public class MockRewardEndpointClient : IRewardEndpointClient
    {
        public Task<bool> CallRewardEndpointAsync(string unityUserId, string placementId, string rewardAdId)
        {
            Debug.Log($"<color=green>[Mock Ad]</color> Validated reward request for {unityUserId} at {placementId}.");
            return Task.FromResult(true);
        }
    }

    public sealed class MockAdsClientModule : IAsyncInitializable
    {
        public MockAdsClientModule(AdManager adManager)
        {
            this.adManager = adManager;
        }

        private readonly AdManager adManager;

        public Task InitializeAsync(CancellationToken ct)
        {
            string userId = AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : "editor_user";
            MockRewardEndpointClient rewardClient = new MockRewardEndpointClient();
            
            adManager.InitializeAds(userId, rewardClient);
            
            return Task.CompletedTask;
        }
    }
}
