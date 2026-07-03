using System;
using System.Collections.Generic;
using GearEngine.Campaign;
using GearEngine.Currency;
using Scaffold.Ads;
using Scaffold.MVVM;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupViewModel : ViewModel
    {
        public ResultPopupViewModel(RaceResultModel result)
        {
            this.result = result ?? throw new ArgumentNullException(nameof(result));
        }

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

        /// <summary>Server band reward (gold) when LiveOps completed the race; otherwise local estimate.</summary>
        public int GoldAmount => result.ServerOutcome != null ? result.ServerOutcome.Reward : result.Gold.Amount;

        public long CurrentGold => currencyClient.GetWallet("gold")?.Current ?? 0;

        public int HighestAchievedTier => result.HighestAchievedTier;

        public IReadOnlyList<ResultStatSlotViewModel> Stats => stats;

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
            if (isProcessingAction) return;
            isProcessingAction = true;
            try
            {
                if (interstitialAdManager != null && await interstitialAdManager.CanShowAd())
                {
                    var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                    void OnAdCompleted(bool success, string p) => tcs.TrySetResult(true);
                    
                    interstitialAdManager.AdSuccessfullyCompleted += OnAdCompleted;
                    interstitialAdManager.ShowInterstitial();
                    
                    await tcs.Task;
                    interstitialAdManager.AdSuccessfullyCompleted -= OnAdCompleted;
                }
                navigation.Open(new RoguelikeViewModel(), true, new NavigationOptions() { CloseAllViews = true });
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
            if (isProcessingAction) return;
            isProcessingAction = true;
            try
            {
                if (interstitialAdManager != null && await interstitialAdManager.CanShowAd())
                {
                    var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                    void OnAdCompleted(bool success, string p) => tcs.TrySetResult(true);
                    
                    interstitialAdManager.AdSuccessfullyCompleted += OnAdCompleted;
                    interstitialAdManager.ShowInterstitial();
                    
                    await tcs.Task;
                    interstitialAdManager.AdSuccessfullyCompleted -= OnAdCompleted;
                }

                if (toolbarController != null) 
                {
                    toolbarController.OpenMainView();
                }
                else
                {
                    navigation.Open(new MainViewModel(), true, new NavigationOptions() { CloseAllViews = true });
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
