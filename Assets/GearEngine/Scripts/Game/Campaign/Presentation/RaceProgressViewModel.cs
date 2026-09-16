using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scaffold.Ads;
using GearEngine.Campaign.Services;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RaceProgressViewModel : ViewModel
    {
        private readonly RaceResultModel result;
        private bool hasContinued;
        [Inject] private ITrackService trackService;
        [Inject] private ToolbarController toolbarController;
        [Inject] private InterstitialAdManager interstitialAdManager;

        public RaceProgressViewModel(RaceResultModel result)
        {
            this.result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public string TrackName => result.TrackName;
        public int HighestAchievedTier => result.HighestAchievedTier;
        public string Summary => HighestAchievedTier > 0 ? $"{HighestAchievedTier} / {result.Tiers.Count} STARS EARNED" : "KEEP RACING TO EARN A STAR";
        public IReadOnlyList<string> TierTargets => result.Tiers.Select(tier => $"{tier.TargetTimeSeconds:0.#}s OR {tier.TargetScore} PTS").ToArray();
        public string NextTrackName => trackService?.GetOrderedTracks()?
            .FirstOrDefault(entry => entry.TrackId == result.ServerOutcome?.NextTrackId)?.Track?.GetDisplayName() ?? string.Empty;
        public string NextTrackMessage => string.IsNullOrEmpty(NextTrackName) ? "CONTINUE YOUR CAMPAIGN" : $"NEXT TRACK · {NextTrackName}";

        public async void Continue()
        {
            if (hasContinued)
            {
                return;
            }

            hasContinued = true;
            try
            {
                await ShowInterstitialIfAvailableAsync();
                if (toolbarController != null)
                {
                    toolbarController.OpenMainView();
                }
                else
                {
                    navigation.Open(new MainViewModel(), true, new NavigationOptions { CloseAllViews = true });
                }
            }
            catch (Exception ex)
            {
                hasContinued = false;
                Debug.LogError($"[RaceProgressViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private async Task ShowInterstitialIfAvailableAsync()
        {
            if (interstitialAdManager == null || !await interstitialAdManager.CanShowAd())
            {
                return;
            }

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnAdCompleted(bool success, string _) => completion.TrySetResult(success);

            interstitialAdManager.AdSuccessfullyCompleted += OnAdCompleted;
            try
            {
                interstitialAdManager.ShowInterstitial();
                await completion.Task;
            }
            finally
            {
                interstitialAdManager.AdSuccessfullyCompleted -= OnAdCompleted;
            }
        }

    }
}
