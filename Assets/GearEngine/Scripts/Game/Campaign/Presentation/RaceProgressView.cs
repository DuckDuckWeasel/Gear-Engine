using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RaceProgressView : View<RaceProgressViewModel>
    {
        [SerializeField] private TMP_Text trackText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text nextTrackText;
        [SerializeField] private TMP_Text[] tierTargetTexts;
        [SerializeField] private TMP_Text[] tierStateTexts;
        [SerializeField] private Image[] stars;
        [SerializeField] private Sprite earnedStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button continueButton;
        [SerializeField] private PostRaceAnimation screenAnimation;

        protected override void OnBind()
        {
            trackText.text = viewModel.TrackName;
            summaryText.text = viewModel.Summary;
            nextTrackText.text = viewModel.NextTrackMessage;
            for (int i = 0; i < tierTargetTexts.Length; i++)
            {
                bool hasTier = i < viewModel.TierTargets.Count;
                tierTargetTexts[i].transform.parent.gameObject.SetActive(hasTier);
                if (!hasTier)
                {
                    continue;
                }

                tierTargetTexts[i].text = viewModel.TierTargets[i];
                tierStateTexts[i].text = i < viewModel.HighestAchievedTier ? "EARNED" : "TARGET";
            }
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
            if (viewModel is System.ComponentModel.INotifyPropertyChanged observable)
            {
                observable.PropertyChanged -= OnViewModelChanged;
            }

            base.OnUnbind();
        }

        protected override void OnOpen(bool wasHidden)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
            continueButton.onClick.AddListener(viewModel.Continue);
            screenAnimation.Play();
        }

        protected override void OnClose(bool hiding)
        {
            continueButton.onClick.RemoveListener(viewModel.Continue);
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
