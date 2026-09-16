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
        [SerializeField] private Sprite gearIcon;
        [SerializeField] private TMP_Text continueText;
        [SerializeField] private Button continueButton;
        [SerializeField] private PostRaceAnimation screenAnimation;

        protected override void OnBind()
        {
            Bind<string, string>(() => viewModel.ContinueLabel, value => continueText.text = value);
            Bind<bool, bool>(() => viewModel.NeedsGearSelection, _ => UpdateIcon());
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
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);

            base.OnUnbind();
        }

        private void UpdateIcon()
        {
            rewardIcon.sprite = viewModel.IsGold ? goldIcon : viewModel.NeedsGearSelection ? gearIcon : viewModel.RewardIcon;
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
            PostRaceViewBindings.Detach(viewModel, OnViewModelChanged);
        }
    }
}
