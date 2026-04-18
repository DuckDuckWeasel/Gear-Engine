using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Tracks;
using GearEngine.GearEngine.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Scaffold.MVVM;

namespace GearEngine.Race.Presentation
{
    public sealed class RaceView : View<RaceViewModel>
    {
        [SerializeField] private Track track;
        [SerializeField] private Button raceButton;

        protected override void OnBind()
        {
            ValidateHierarchy();

            BindGearEngine();
            BindTrack();
            SubscribeRaceUi();
        }

        protected override void OnUnbind()
        {
            UnsubscribeRaceUi();
            if (track != null)
            {
                track.ReleaseViewBinding();
            }

            base.OnUnbind();
        }

        // ── Binding ─────────────────────────────────────────────

        private void BindGearEngine()
        {
            //gearEngineView.Bind(viewModel.GearEngine);
        }

        private void BindTrack()
        {
            track.Bind(viewModel.Track);
        }

        private void SubscribeRaceUi()
        {
            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => viewModel.Track.State, OnTrackStateChanged);
            raceButton.onClick.AddListener(OnRaceButtonClicked);
        }

        // ── Race UI Handlers ────────────────────────────────────

        private void OnRaceButtonClicked()
        {
            try
            {
                viewModel?.ToggleRace();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaceView] ToggleRace failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnTrackStateChanged(SimulationLifecycleState state)
        {
            TextMeshProUGUI label = raceButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = state == SimulationLifecycleState.Running ? "Stop" : "Start";
            }
        }

        // ── Cleanup ─────────────────────────────────────────────

        private void UnsubscribeRaceUi()
        {
            if (raceButton != null)
            {
                raceButton.onClick.RemoveListener(OnRaceButtonClicked);
            }
        }

        private void ValidateHierarchy()
        {
            ThrowIfMissing(track, "track");
            ThrowIfMissing(raceButton, "raceButton");
        }

        private static void ThrowIfMissing(object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[RaceView] {name} reference is missing.");
            }
        }
    }
}
