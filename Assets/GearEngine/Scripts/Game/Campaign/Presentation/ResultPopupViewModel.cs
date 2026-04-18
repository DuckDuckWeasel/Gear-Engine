using System;
using GearEngine.Campaign;
using GearEngine.Campaign.Services;
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
        public int GoldAmount => result.Gold.Amount;

        public int CurrentGold => walletService.CurrentGold;

        private readonly RaceResultModel result;

        [Inject] private ITrackService trackService;
        [Inject] private IWalletService walletService;

        public void Upgrade()
        {
            try
            {
                navigation.Open(new RoguelikeViewModel());
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
                if (result.IsGoodResult)
                {
                    trackService.AdvanceToNextTrack();
                }

                navigation.Open(new MainViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResultPopupViewModel] Continue failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
