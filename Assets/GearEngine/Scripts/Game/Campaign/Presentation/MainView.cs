using System;
using GearEngine.CarSimulation.Tracks;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class MainView : View<MainViewModel>
    {
        [SerializeField] private Track track;
        [SerializeField] private Button playButton;
        [SerializeField] private TrackStatsViewComponent statsPanel;

        protected override void OnBind()
        {
            ValidateHierarchy();
            track.Bind(viewModel.Track);
            statsPanel.Bind(viewModel.Stats);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        protected override void OnUnbind()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            track.Unbind();
            base.OnUnbind();
        }

        private void OnPlayClicked()
        {
            try
            {
                viewModel?.GoToSetup();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainView] OnPlayClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(track, nameof(track));
            RequireReference(playButton, nameof(playButton));
            RequireReference(statsPanel, nameof(statsPanel));
        }

        private void RequireReference(UnityEngine.Object field, string name)
        {
            if (field == null)
            {
                throw new InvalidOperationException($"[MainView] {name} reference is missing.");
            }
        }
    }
}
