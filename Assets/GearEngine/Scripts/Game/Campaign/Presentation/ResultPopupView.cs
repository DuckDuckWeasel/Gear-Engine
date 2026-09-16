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
        [SerializeField] private Image[] stars;
        [SerializeField] private Sprite earnedStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button continueButton;
        [SerializeField] private ResultStandingsView standings;
        [SerializeField] private PostRaceAnimation screenAnimation;

        protected override void OnBind()
        {
            titleText.text = viewModel.VictoryTitle;
            scoreText.text = viewModel.Score.ToString();
            trackText.text = viewModel.TrackName;
            standings.Bind(viewModel.Standings);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < viewModel.HighestAchievedTier ? earnedStar : emptyStar;
            }

            continueButton.onClick.AddListener(viewModel.Continue);
        }

        protected override void OnUnbind()
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            screenAnimation.Stop();
            standings.Stop();
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);

            base.OnUnbind();
        }

        protected override void OnOpen(bool wasHidden)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            continueButton.onClick.AddListener(viewModel.Continue);
            screenAnimation.Play();
            standings.Play();
        }

        protected override void OnClose(bool hiding)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            screenAnimation.Stop();
            standings.Stop();
        }

        private void OnDestroy()
        {
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);
        }
    }
}
