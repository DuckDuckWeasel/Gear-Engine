using System;
using System.Threading.Tasks;
using Scaffold.Ads;
using VContainer;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ReceivedRewardsViewModel : ViewModel
    {
        public ReceivedRewardsViewModel(RaceResultModel result, IItem receivedReward = null)
        {
            this.result = result ?? throw new ArgumentNullException(nameof(result));
            ReceivedReward = receivedReward;
            if (receivedReward != null && GoldAmount > 0)
            {
                index = 1;
            }
        }

        public IItem ReceivedReward { get; }
        public int GoldAmount => result.ServerOutcome?.Reward ?? result.Gold.Amount;
        public int RewardCount => (GoldAmount > 0 ? 1 : 0) + (result.HasGearReward || ReceivedReward != null ? 1 : 0);
        public bool IsGold => GoldAmount > 0 && index == 0;
        public bool NeedsGearSelection => !IsGold && ReceivedReward == null && result.HasGearReward;
        public string ContinueLabel => NeedsGearSelection ? "UPGRADE" : "CONTINUE";
        public string RewardName => RewardCount == 0 ? "NO REWARDS THIS RUN" : IsGold ? $"{GoldAmount} GOLD" : NeedsGearSelection ? "GEAR UPGRADE" : ReceivedReward.Name;
        public Sprite RewardIcon => IsGold ? null : ReceivedReward?.Icon;
        public string RewardCountText => RewardCount == 0 ? "KEEP RACING" : $"REWARD {index + 1}/{RewardCount}";

        private readonly RaceResultModel result;
        private int index;
        private bool hasContinued;
        [Inject] private InterstitialAdManager interstitialAdManager;


        public async void Continue()
        {
            if (hasContinued || TryAdvanceReward())
            {
                return;
            }

            hasContinued = true;
            try
            {
                await OpenNextScreenAsync();
            }
            catch (Exception ex)
            {
                hasContinued = false;
                Debug.LogError($"[ReceivedRewardsViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool TryAdvanceReward()
        {
            if (NeedsGearSelection || index + 1 >= RewardCount)
            {
                return false;
            }

            index++;
            NotifyRewardChanged();
            return true;
        }

        private void NotifyRewardChanged()
        {
            OnPropertyChanged(nameof(RewardName));
            OnPropertyChanged(nameof(RewardIcon));
            OnPropertyChanged(nameof(IsGold));
            OnPropertyChanged(nameof(RewardCountText));
            OnPropertyChanged(nameof(NeedsGearSelection));
            OnPropertyChanged(nameof(ContinueLabel));
        }

        private async Task OpenNextScreenAsync()
        {
            if (NeedsGearSelection)
            {
                await ShowInterstitialIfAvailableAsync();
                navigation.Open(new RoguelikeViewModel(result), true, new NavigationOptions { CloseAllViews = true });
                return;
            }
            navigation.Open(new RaceProgressViewModel(result), true, new NavigationOptions { CloseAllViews = true });
        }

        private async Task ShowInterstitialIfAvailableAsync()
        {
            if (interstitialAdManager == null || !await interstitialAdManager.CanShowAd())
            {
                return;
            }

            await WaitForInterstitialAsync();
        }

        private async Task WaitForInterstitialAsync()
        {
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
