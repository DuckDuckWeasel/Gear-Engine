using System;
using System.Collections.Generic;
using GearEngine.Campaign;
using GearEngine.Currency;
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

        public string PositionLabel => $"{GetOrdinal(result.Position)} Place";
        
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

        public IReadOnlyList<ResultStatSlotViewModel> Stats => stats;

        private readonly RaceResultModel result;

        private List<ResultStatSlotViewModel> stats;

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

        public void Upgrade()
        {
            try
            {
                navigation.Open(new RoguelikeViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupViewModel] Upgrade failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void Continue()
        {
            try
            {
                navigation.Open(new MainViewModel(), true, new NavigationOptions() { CloseAllViews = true });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private List<ResultStatSlotViewModel> BuildStatsRows()
        {
            string goldLine = $"+{GoldAmount} gold (total: {CurrentGold})";
            return new List<ResultStatSlotViewModel>
            {
                new ResultStatSlotViewModel("Position", PositionLabel),
                new ResultStatSlotViewModel("Race time", FormattedRaceTime),
                new ResultStatSlotViewModel("Laps", result.LapCount.ToString()),
                new ResultStatSlotViewModel("Score", result.Score.ToString()),
                new ResultStatSlotViewModel("Gold", goldLine),
            };
        }

        private string GetOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }
}
