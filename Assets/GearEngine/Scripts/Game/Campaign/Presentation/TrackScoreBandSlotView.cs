using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackScoreBandSlotView : ViewComponent<TrackScoreBandViewModel>
    {
        [SerializeField] private TextMeshProUGUI positionLabel;
        [SerializeField] private TextMeshProUGUI targetTimeLabel;
        [SerializeField] private TextMeshProUGUI rewardLabel;

        protected override void OnBind()
        {
            base.OnBind();

            if (positionLabel != null)
            {
                positionLabel.text = viewModel.Position.ToString();
            }

            if (targetTimeLabel != null)
            {
                System.TimeSpan time = System.TimeSpan.FromSeconds(viewModel.MaxRaceTimeSeconds);
                targetTimeLabel.text = $"{(int)time.TotalSeconds:00}:{time:ff}";
            }

            if (rewardLabel != null)
            {
                rewardLabel.text = viewModel.RewardValue.ToString();
            }
        }
    }
}
