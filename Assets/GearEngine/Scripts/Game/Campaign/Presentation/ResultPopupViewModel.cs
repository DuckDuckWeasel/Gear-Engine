using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.Campaign;
using GearEngine.Currency;
using GearEngine.GearEngine.Services.Inventory;
using Scaffold.Ads;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupViewModel : ViewModel
    {
        public ResultPopupViewModel(
            RaceResultModel result,
            ResultFlowStage initialStage = ResultFlowStage.Victory,
            IItem receivedReward = null)
        {
            this.result = result ?? throw new ArgumentNullException(nameof(result));
            CurrentStage = initialStage;
            ReceivedReward = receivedReward;
        }

        public event Action<ResultFlowStage> StageChanged;

        public float RaceTime => result.RaceTime;

        public int LapCount => result.LapCount;

        public int Score => result.Score;

        public string FormattedRaceTime
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(result.RaceTime);
                return $"{(int)time.TotalSeconds:00}:{time:ff}";
            }
        }

        public int GoldAmount => result.ServerOutcome != null ? result.ServerOutcome.Reward : result.Gold.Amount;

        public long CurrentGold => currencyClient.GetWallet("gold")?.Current ?? 0;

        public int HighestAchievedTier => result.HighestAchievedTier;

        public IReadOnlyList<ResultStatSlotViewModel> Stats => stats;

        public ResultFlowStage CurrentStage { get; private set; }

        public IItem ReceivedReward { get; }

        public string VictoryTitle => result.IsGoodResult ? "1st place" : "Race finished";

        public string VictoryEyebrow => result.IsGoodResult ? "P H O T O  F I N I S H" : "R U N  C O M P L E T E";

        public string VictoryMessage => $"Finished {LapCount} laps in {FormattedRaceTime}.";

        public string RewardName => ReceivedReward?.Name ?? $"{GoldAmount} GOLD";

        public Sprite RewardIcon => ReceivedReward?.Icon;

        public string RewardCountText => ReceivedReward == null ? "REWARD 1/1" : "REWARD 2/2";

        public string ProgressTitle => HasUnlockedTrack ? "NEW TRACK UNLOCKED" : "RACE PROGRESS";

        public string ProgressTrackName => HasUnlockedTrack ? result.ServerOutcome.NextTrackId : result.TrackName;

        public string ProgressSummary => HighestAchievedTier > 0
            ? $"TIER {HighestAchievedTier} COMPLETE"
            : "KEEP RACING TO EARN A STAR";

        private bool HasUnlockedTrack => !string.IsNullOrEmpty(result.ServerOutcome?.NextTrackId);

        private readonly RaceResultModel result;

        private List<ResultStatSlotViewModel> stats;
        private bool isProcessingAction;

        [Inject] private CurrencyClientModule currencyClient;
        [Inject] private ToolbarController toolbarController;
        [Inject] private InterstitialAdManager interstitialAdManager;

        protected override void Initialize()
        {
            base.Initialize();
            stats = BuildStatsRows();
            foreach (ResultStatSlotViewModel row in stats)
            {
                BindChildViewModel(row);
            }
        }

        public async void Upgrade()
        {
            if (isProcessingAction)
            {
                return;
            }

            isProcessingAction = true;
            try
            {
                await result.PersistenceCompleted;
                await ShowInterstitialIfAvailableAsync();
                navigation.Open(
                    new RoguelikeViewModel(result),
                    true,
                    new NavigationOptions { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupViewModel] Upgrade failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                isProcessingAction = false;
            }
        }

        public async void Continue()
        {
            if (isProcessingAction)
            {
                return;
            }

            isProcessingAction = true;
            try
            {
                await result.PersistenceCompleted;
                switch (CurrentStage)
                {
                    case ResultFlowStage.Victory:
                        SetStage(ResultFlowStage.Reward);
                        break;
                    case ResultFlowStage.Reward:
                        SetStage(ResultFlowStage.Progress);
                        break;
                    case ResultFlowStage.Progress:
                        await ShowInterstitialIfAvailableAsync();
                        OpenMainView();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                isProcessingAction = false;
            }
        }

        private void SetStage(ResultFlowStage stage)
        {
            CurrentStage = stage;
            StageChanged?.Invoke(stage);
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

        private void OpenMainView()
        {
            if (toolbarController != null)
            {
                toolbarController.OpenMainView();
                return;
            }

            navigation.Open(
                new MainViewModel(),
                true,
                new NavigationOptions { CloseAllViews = true });
        }

        private List<ResultStatSlotViewModel> BuildStatsRows()
        {
            string goldLine = $"+{GoldAmount} gold (total: {CurrentGold})";
            string tierLine = HighestAchievedTier > 0 ? $"Tier {HighestAchievedTier}" : "None";
            return new List<ResultStatSlotViewModel>
            {
                new ResultStatSlotViewModel("Tier Achieved", tierLine),
                new ResultStatSlotViewModel("Race time", FormattedRaceTime),
                new ResultStatSlotViewModel("Laps", result.LapCount.ToString()),
                new ResultStatSlotViewModel("Score", result.Score.ToString()),
                new ResultStatSlotViewModel("Gold", goldLine),
            };
        }
    }
}
