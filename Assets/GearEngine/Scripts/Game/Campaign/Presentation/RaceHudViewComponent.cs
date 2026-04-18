using Scaffold.MVVM;
using TMPro;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RaceHudViewComponent : ViewComponent<ActiveRaceViewModel>
    {
        [SerializeField] private TextMeshProUGUI lapTimeLabel;
        [SerializeField] private TextMeshProUGUI lapCountLabel;

        protected override void OnBind()
        {
            base.OnBind();
            Bind<float, float>(() => viewModel.Track.HudRaceTime, OnHudMetricsChanged);
            Bind<int, int>(() => viewModel.Track.HudCurrentLap, OnHudMetricsChanged);
        }

        private void OnHudMetricsChanged<T>(T _)
        {
            if (lapTimeLabel != null)
            {
                lapTimeLabel.text = $"{viewModel.Track.HudRaceTime:F2}s";
            }

            if (lapCountLabel != null)
            {
                lapCountLabel.text = $"Lap {viewModel.Track.HudCurrentLap}";
            }
        }
    }
}
