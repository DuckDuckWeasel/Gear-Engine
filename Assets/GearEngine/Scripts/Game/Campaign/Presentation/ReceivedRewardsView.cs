using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ReceivedRewardsView : View<ReceivedRewardsViewModel>
    {
        [SerializeField] private TMP_Text rewardNameText;
        [SerializeField] private TMP_Text rewardCountText;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Sprite goldIcon;
        [SerializeField] private Button continueButton;
        [SerializeField] private PostRaceAnimation screenAnimation;

        protected override void OnBind()
        {
            Bind<string, string>(() => viewModel.RewardName, value => rewardNameText.text = value);
            Bind<string, string>(() => viewModel.RewardCountText, value => rewardCountText.text = value);
            Bind<Sprite, Sprite>(() => viewModel.RewardIcon, _ => UpdateIcon());
            Bind<bool, bool>(() => viewModel.IsGold, _ => UpdateIcon());
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

        private void UpdateIcon()
        {
            rewardIcon.sprite = viewModel.IsGold ? goldIcon : viewModel.RewardIcon;
            rewardIcon.enabled = rewardIcon.sprite != null;
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
