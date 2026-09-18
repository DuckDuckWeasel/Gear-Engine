using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Tracks;
using GearEngine.FrustumFit;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class MainView : View<MainViewModel>
    {
        [SerializeField] private TrackViewComponent track;
        [SerializeField] private Button playButton;
        [SerializeField] private Button previousTrackButton;
        [SerializeField] private Button nextTrackButton;
        [SerializeField] private TextMeshProUGUI trackPositionLabel;
        private readonly Dictionary<Renderer, MaterialPropertyBlock> originalTrackProperties = new Dictionary<Renderer, MaterialPropertyBlock>();
        [SerializeField] private Button talentPerksButton;
        [SerializeField] private Button gearsButton;
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

            previousTrackButton.onClick.AddListener(viewModel.PreviousTrack);
            nextTrackButton.onClick.AddListener(viewModel.NextTrack);
            Bind<CampaignTrackPreviewViewModel, CampaignTrackPreviewViewModel>(() => viewModel.Track, UpdateTrackPreview);
            Bind<TrackStatsViewModel, TrackStatsViewModel>(() => viewModel.Stats, UpdateTrackStats);
            Bind<bool, bool>(() => viewModel.CanNavigateTracks, UpdateTrackNavigation);
            Bind<string, string>(() => viewModel.TrackPosition, value => trackPositionLabel.text = value);
            Bind<bool, bool>(() => viewModel.IsTrackLocked, UpdateTrackLock);
        }

        protected override void OnOpen(bool wasHidden)
        {
            base.OnOpen(wasHidden);
            viewModel.RefreshTracks();
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
            if (talentPerksButton != null)
            {
                talentPerksButton.onClick.RemoveListener(OnTalentPerksClicked);
                talentPerksButton.onClick.AddListener(OnTalentPerksClicked);
            }
            if (gearsButton != null)
            {
                gearsButton.onClick.RemoveListener(OnGearsClicked);
                gearsButton.onClick.AddListener(OnGearsClicked);
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
            if (gearsButton != null)
            {
                gearsButton.onClick.RemoveListener(OnGearsClicked);
            }

            RestoreTrackAppearance();
            if (track != null)
            {
                track.gameObject.SetActive(false);
            }
        }

        protected override void OnUnbind()
        {
            previousTrackButton.onClick.RemoveListener(viewModel.PreviousTrack);
            nextTrackButton.onClick.RemoveListener(viewModel.NextTrack);
            RestoreTrackAppearance();
            track.Unbind();
            base.OnUnbind();
        }

        private void UpdateTrackPreview(CampaignTrackPreviewViewModel preview)
        {
            RestoreTrackAppearance();
            if (preview != null)
            {
                track.Bind(preview);
            }

            UpdateTrackLock(viewModel.IsTrackLocked);
        }

        private void UpdateTrackStats(TrackStatsViewModel stats)
        {
            if (stats != null)
            {
                statsPanel.Bind(stats);
            }

            if (viewModel.IsTrackLocked)
            {
                statsPanel.ShowLockedTrack();
            }
        }

        private void UpdateTrackNavigation(bool canNavigate)
        {
            previousTrackButton.gameObject.SetActive(canNavigate);
            nextTrackButton.gameObject.SetActive(canNavigate);
        }

        private void UpdateTrackLock(bool isLocked)
        {
            playButton.interactable = !isLocked;
            RestoreTrackAppearance();
            if (!isLocked)
            {
                return;
            }

            statsPanel.ShowLockedTrack();
            foreach (Renderer renderer in track.GetComponentsInChildren<Renderer>(true))
            {
                SetTrackRendererBlack(renderer);
            }
        }

        private void SetTrackRendererBlack(Renderer renderer)
        {
            MaterialPropertyBlock original = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(original);
            originalTrackProperties[renderer] = original;
            MaterialPropertyBlock black = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(black);
            black.SetColor("_BaseColor", Color.black);
            black.SetColor("_Color", Color.black);
            black.SetColor("_EmissionColor", Color.black);
            renderer.SetPropertyBlock(black);
        }

        private void RestoreTrackAppearance()
        {
            foreach (KeyValuePair<Renderer, MaterialPropertyBlock> entry in originalTrackProperties)
            {
                if (entry.Key != null)
                {
                    entry.Key.SetPropertyBlock(entry.Value);
                }
            }

            originalTrackProperties.Clear();
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

        private void OnGearsClicked()
        {
            try
            {
                viewModel?.ClickedGears();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainView] OnGearsClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ValidateHierarchy()
        {
            RequireReference(playButton, nameof(playButton));
            RequireReference(statsPanel, nameof(statsPanel));
            RequireReference(previousTrackButton, nameof(previousTrackButton));
            RequireReference(nextTrackButton, nameof(nextTrackButton));
            RequireReference(trackPositionLabel, nameof(trackPositionLabel));
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
