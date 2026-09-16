using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}.{time.Milliseconds / 10:00}";
            }
        }

        public int GoldAmount => result.ServerOutcome != null ? result.ServerOutcome.Reward : result.Gold.Amount;

        public long CurrentGold => currencyClient?.GetWallet("gold")?.Current ?? 0;

        public int HighestAchievedTier => result.HighestAchievedTier;

        public IReadOnlyList<ResultStatSlotViewModel> Stats => stats;

        public string VictoryTitle => "RESULTS";

        public string TrackName => result.TrackName;
        public RaceStandingsModel Standings => result.Standings;

        private readonly RaceResultModel result;

        private List<ResultStatSlotViewModel> stats;
        private bool isProcessingAction;

        [Inject] private CurrencyClientModule currencyClient;


        protected override void Initialize()
        {
            base.Initialize();
            stats = BuildStatsRows();
            foreach (ResultStatSlotViewModel row in stats)
            {
                BindChildViewModel(row);
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
                navigation.Open(new ReceivedRewardsViewModel(result), true, new NavigationOptions { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                isProcessingAction = false;
                Debug.LogError($"[ResultPopupViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
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
