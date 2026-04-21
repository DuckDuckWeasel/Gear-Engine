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
            string resultLabel = result.IsGoodResult ? "Win" : "Try again";
            string goldLine = $"+{GoldAmount} gold (total: {CurrentGold})";
            return new List<ResultStatSlotViewModel>
            {
                new ResultStatSlotViewModel("Result", resultLabel),
                new ResultStatSlotViewModel("Race time", $"{result.RaceTime:F1}s"),
                new ResultStatSlotViewModel("Laps", result.LapCount.ToString()),
                new ResultStatSlotViewModel("Score", result.Score.ToString()),
                new ResultStatSlotViewModel("Gold", goldLine),
            };
        }
    }
}
