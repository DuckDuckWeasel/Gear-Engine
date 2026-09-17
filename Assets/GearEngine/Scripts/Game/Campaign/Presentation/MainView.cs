using System;
using System.Collections;
using DG.Tweening;
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
        [SerializeField] private Button talentPerksButton;
        [SerializeField] private Button gearsButton;
        [SerializeField] private TrackStatsViewComponent statsPanel;
        [SerializeField] private FrustumFitAnchor[] openTransitionAnchors;
        [SerializeField] private float openTransitionDurationSeconds = 0.35f;

        private Coroutine previewRoutine;
        private Tween previewTween;
        private Vector3 originalTrackPosition;
        private Quaternion originalTrackRotation;
        private Vector3 originalTrackScale;
        private bool hasTrackSnapshot;
        private TextMeshProUGUI playButtonLabel;

        protected override void OnBind()
        {
            ValidateHierarchy();
            if (track == null)
            {
                throw new InvalidOperationException(
                    "[MainView] Track must be assigned on the scene instance (not baked into the prefab).");
            }

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
            RegisterButtonListeners();
            track.gameObject.SetActive(true);
        }

        protected override void OnClose(bool hiding)
        {
            base.OnClose(hiding);
            RestoreTrackPose();
            UnregisterButtonListeners();
            if (hiding)
            {
                return;
            }

            if (track != null)
            {
                track.gameObject.SetActive(false);
            }
        }

        protected override void OnUnbind()
        {
            UnregisterButtonListeners();
            RestoreTrackPose();
            track.Unbind();
            base.OnUnbind();
        }

        private void UpdateTrackPreview(CampaignTrackPreviewViewModel preview)
        {
            RestoreTrackPose();
            if (preview == null)
            {
                track.gameObject.SetActive(false);
                return;
            }

            track.gameObject.SetActive(true);
            track.Bind(preview);
            if (isActiveAndEnabled)
            {
                StartTrackPreview();
            }
        }

        private void UpdateTrackStats(TrackStatsViewModel stats)
        {
            if (stats != null)
            {
                statsPanel.Bind(stats);
                statsPanel.SetLocked(viewModel.IsTrackLocked);
            }
        }

        private void UpdateTrackNavigation(bool canNavigate)
        {
            previousTrackButton.gameObject.SetActive(canNavigate);
            nextTrackButton.gameObject.SetActive(canNavigate);
            trackPositionLabel.gameObject.SetActive(canNavigate);
        }

        private void UpdateTrackLock(bool isLocked)
        {
            playButton.interactable = !isLocked && viewModel.Track != null;
            playButtonLabel.text = isLocked ? "LOCKED" : "RACE";
            statsPanel.SetLocked(isLocked);
        }

        private void RegisterButtonListeners()
        {
            UnregisterButtonListeners();
            playButton.onClick.AddListener(OnPlayClicked);
            previousTrackButton.onClick.AddListener(viewModel.PreviousTrack);
            nextTrackButton.onClick.AddListener(viewModel.NextTrack);
            talentPerksButton?.onClick.AddListener(OnTalentPerksClicked);
            gearsButton?.onClick.AddListener(OnGearsClicked);
        }

        private void UnregisterButtonListeners()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            previousTrackButton.onClick.RemoveListener(viewModel.PreviousTrack);
            nextTrackButton.onClick.RemoveListener(viewModel.NextTrack);
            talentPerksButton?.onClick.RemoveListener(OnTalentPerksClicked);
            gearsButton?.onClick.RemoveListener(OnGearsClicked);
        }

        private void StartTrackPreview()
        {
            RestoreTrackPose();
            originalTrackPosition = track.transform.position;
            originalTrackRotation = track.transform.rotation;
            originalTrackScale = track.transform.localScale;
            hasTrackSnapshot = true;
            previewRoutine = StartCoroutine(FitTrackAfterLayout());
        }

        private IEnumerator FitTrackAfterLayout()
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            previewTween = FrustumFitAnchorOpenTransition.Play(openTransitionAnchors, openTransitionDurationSeconds);
            previewRoutine = null;
        }

        private void RestoreTrackPose()
        {
            if (previewRoutine != null)
            {
                StopCoroutine(previewRoutine);
                previewRoutine = null;
            }
            previewTween?.Kill();
            previewTween = null;
            if (!hasTrackSnapshot || track == null)
            {
                return;
            }
            // The track is shared with Setup and Race; Home fitting must not change their world scale.
            track.transform.SetPositionAndRotation(originalTrackPosition, originalTrackRotation);
            track.transform.localScale = originalTrackScale;
            hasTrackSnapshot = false;
        }

        private void OnDisable() => RestoreTrackPose();

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

            playButtonLabel = playButton.GetComponentInChildren<TextMeshProUGUI>(true);
            RequireReference(playButtonLabel, nameof(playButtonLabel));
            ConfigureTrackStatusLabel();
        }

        private void ConfigureTrackStatusLabel()
        {
            RectTransform statusRect = trackPositionLabel.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 0.585f);
            statusRect.anchorMax = statusRect.anchorMin;
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(760f, 96f);
            trackPositionLabel.enableAutoSizing = true;
            trackPositionLabel.fontSizeMin = 22f;
            trackPositionLabel.fontSizeMax = 34f;
            trackPositionLabel.alignment = TextAlignmentOptions.Center;
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
