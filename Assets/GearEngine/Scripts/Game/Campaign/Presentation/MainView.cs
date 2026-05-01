using System;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class MainView : View<MainViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private Button playButton;
        [SerializeField] private Button talentPerksButton;
        [SerializeField] private TrackStatsViewComponent statsPanel;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        protected override void OnBind()
        {
            ValidateHierarchy();
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[MainView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

            track.Bind(viewModel.Track);
            statsPanel.Bind(viewModel.Stats);
        }

        protected override void OnOpen(bool wasHidden)
        {
            base.OnOpen(wasHidden);
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
            if (talentPerksButton != null)
            {
                talentPerksButton.onClick.RemoveListener(OnTalentPerksClicked);
                talentPerksButton.onClick.AddListener(OnTalentPerksClicked);
            }
            track.gameObject.SetActive(true);
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(this, openTransitionAnchors, openTransitionDurationSeconds);
        }

        protected override void OnClose(bool hiding)
        {
            base.OnClose(hiding);
            if (hiding)
            {
                return;
            }

            playButton.onClick.RemoveListener(OnPlayClicked);
            if (talentPerksButton != null)
            {
                talentPerksButton.onClick.RemoveListener(OnTalentPerksClicked);
            }

            if (track != null)
            {
                track.gameObject.SetActive(false);
            }
        }

        private void OnPlayClicked()
        {
            try
            {
                viewModel?.ClickedPlay();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainView] OnPlayClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnTalentPerksClicked()
        {
            try
            {
                viewModel?.ClickedTalentPerks();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainView] OnTalentPerksClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
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
