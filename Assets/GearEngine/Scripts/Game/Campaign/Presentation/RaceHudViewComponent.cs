using GearEngine.CarSimulation;
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
            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => viewModel.Track.State, OnTrackStateChanged);
        }

        private void OnTrackStateChanged(SimulationLifecycleState _)
        {
            if (lapTimeLabel != null)
            {
                lapTimeLabel.text = $"{viewModel.Track.Session.RaceTime:F2}s";
            }

            if (lapCountLabel != null)
            {
                lapCountLabel.text = $"Lap {viewModel.Track.Session.CurrentLap}";
            }
        }
    }
}
