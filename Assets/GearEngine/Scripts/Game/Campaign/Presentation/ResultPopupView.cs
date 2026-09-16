using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ResultPopupView : View<ResultPopupViewModel>
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text trackText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text lapsText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private Image[] stars;
        [SerializeField] private Sprite earnedStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private PostRaceAnimation screenAnimation;

        protected override void OnBind()
        {
            titleText.text = viewModel.VictoryTitle;
            scoreText.text = viewModel.Score.ToString();
            trackText.text = viewModel.TrackName;
            timeText.text = viewModel.FormattedRaceTime;
            lapsText.text = viewModel.LapCount.ToString();
            goldText.text = viewModel.GoldAmount.ToString();
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < viewModel.HighestAchievedTier ? earnedStar : emptyStar;
            }

            continueButton.onClick.AddListener(viewModel.Continue);
            upgradeButton.onClick.AddListener(viewModel.Upgrade);
        }

        protected override void OnUnbind()
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            upgradeButton.onClick.RemoveListener(viewModel.Upgrade);
            screenAnimation.Stop();
            if (viewModel is System.ComponentModel.INotifyPropertyChanged observable)
            {
                observable.PropertyChanged -= OnViewModelChanged;
            }

            base.OnUnbind();
        }

        protected override void OnOpen(bool wasHidden)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            upgradeButton.onClick.RemoveListener(viewModel.Upgrade);
            continueButton.onClick.AddListener(viewModel.Continue);
            upgradeButton.onClick.AddListener(viewModel.Upgrade);
            screenAnimation.Play();
        }

        protected override void OnClose(bool hiding)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            upgradeButton.onClick.RemoveListener(viewModel.Upgrade);
            screenAnimation.Stop();
        }

        private void OnDestroy()
        {
            if (viewModel is System.ComponentModel.INotifyPropertyChanged observable)
            {
                observable.PropertyChanged -= OnViewModelChanged;
            }
        }
    }
}
