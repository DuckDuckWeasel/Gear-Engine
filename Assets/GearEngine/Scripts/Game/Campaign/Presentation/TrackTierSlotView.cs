using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackTierSlotView : ViewComponent<TrackTierViewModel>
    {
        [SerializeField] private TextMeshProUGUI tierLabel;
        [SerializeField] private TextMeshProUGUI targetLabel;
        [SerializeField] private TextMeshProUGUI rewardLabel;

        protected override void OnBind()
        {
            base.OnBind();

            if (tierLabel != null)
            {
                tierLabel.text = viewModel.TierNumber.ToString();
            }

            if (targetLabel != null)
            {
                targetLabel.text = viewModel.TargetDescription;
            }

            if (rewardLabel != null)
            {
                rewardLabel.text = viewModel.RewardDescription;
            }
        }
    }
}
