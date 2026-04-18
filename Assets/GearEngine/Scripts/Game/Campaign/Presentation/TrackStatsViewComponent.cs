using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackStatsViewComponent : ViewComponent<TrackStatsViewModel>
    {
        [SerializeField] private TextMeshProUGUI trackNameLabel;
        [SerializeField] private TextMeshProUGUI targetLapsLabel;
        [SerializeField] private TextMeshProUGUI targetTimeLabel;

        protected override void OnBind()
        {
            base.OnBind();
            if (trackNameLabel != null)
            {
                trackNameLabel.text = viewModel.TrackName;
            }

            if (targetLapsLabel != null)
            {
                string lapsText = viewModel.TargetLaps < 0 ? "Laps: —" : $"Laps: {viewModel.TargetLaps}";
                targetLapsLabel.text = lapsText;
            }

            if (targetTimeLabel != null)
            {
                targetTimeLabel.text = $"Target: {viewModel.TargetTime:F1}s";
            }
        }
    }
}
