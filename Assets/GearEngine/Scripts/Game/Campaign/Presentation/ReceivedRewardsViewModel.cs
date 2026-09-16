using System;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ReceivedRewardsViewModel : ViewModel
    {
        private readonly RaceResultModel result;
        private int index;
        private bool hasContinued;

        public ReceivedRewardsViewModel(RaceResultModel result, IItem receivedReward = null)
        {
            this.result = result ?? throw new ArgumentNullException(nameof(result));
            ReceivedReward = receivedReward;
        }

        public IItem ReceivedReward { get; }
        public int GoldAmount => result.ServerOutcome?.Reward ?? result.Gold.Amount;
        public int RewardCount => (GoldAmount > 0 ? 1 : 0) + (ReceivedReward == null ? 0 : 1);
        public bool IsGold => GoldAmount > 0 && index == 0;
        public string RewardName => RewardCount == 0 ? "NO REWARDS THIS RUN" : IsGold ? $"{GoldAmount} GOLD" : ReceivedReward.Name;
        public Sprite RewardIcon => IsGold ? null : ReceivedReward?.Icon;
        public string RewardCountText => RewardCount == 0 ? "KEEP RACING" : $"REWARD {index + 1}/{RewardCount}";

        public void Continue()
        {
            if (hasContinued)
            {
                return;
            }

            if (index + 1 < RewardCount)
            {
                index++;
                OnPropertyChanged(nameof(RewardName));
                OnPropertyChanged(nameof(RewardIcon));
                OnPropertyChanged(nameof(IsGold));
                OnPropertyChanged(nameof(RewardCountText));
                return;
            }

            hasContinued = true;
            try
            {
                navigation.Open(new RaceProgressViewModel(result), true,
                    new NavigationOptions { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                hasContinued = false;
                Debug.LogError($"[ReceivedRewardsViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
