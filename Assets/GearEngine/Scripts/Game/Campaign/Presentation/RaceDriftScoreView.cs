using UnityEngine;
using TMPro;
using DG.Tweening;
using Scaffold.MVVM;
using System.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RaceDriftScoreView : ViewComponent<RaceDriftScoreViewModel>
    {
        private static readonly Color HudTextColor = new Color32(44, 57, 69, 255);

        [Header("References")]
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text totalScoreText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Juice Settings")]
        [SerializeField] private Vector3 multiplierPunchScale = new Vector3(0.5f, 0.5f, 0f);
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private float pointScalePingPong = 1.05f;
        [SerializeField] private float pointPingPongDuration = 0.2f;
        [SerializeField] private float punchScalePerTier = 0.2f;

        private Tween pointsTween;
        private Tween fadeTween;
        private Tween multiplierPunchTween;
        private Tween multiplierLoopTween;

        protected override void OnBind()
        {
            base.OnBind();

            viewModel.MultiplierIncreased += OnMultiplierIncreased;
            viewModel.ScoreBanked += OnScoreBanked;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            UpdateVisibility(false, false);
            UpdateMultiplierTextAndColor();
            UpdatePointsText();
            UpdateTotalScoreText();
        }

        protected override void OnUnbind()
        {
            if (viewModel != null)
            {
                viewModel.MultiplierIncreased -= OnMultiplierIncreased;
                viewModel.ScoreBanked -= OnScoreBanked;
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            KillTweens();
            base.OnUnbind();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.IsDisplayingScore))
            {
                UpdateVisibility(viewModel.IsDisplayingScore, true);
            }
            else if (e.PropertyName == nameof(viewModel.DisplayPoints))
            {
                UpdatePointsText();
                UpdateTotalScoreText();
            }
            else if (e.PropertyName == nameof(viewModel.CurrentMultiplier))
            {
                UpdateMultiplierTextAndColor();
                UpdateTotalScoreText();
            }
        }

        private void UpdateTotalScoreText()
        {
            if (totalScoreText == null)
            {
                return;
            }

            int displayedTotal = viewModel.TotalDriftScore + (viewModel.DisplayPoints * viewModel.CurrentMultiplier);
            totalScoreText.text = $"{displayedTotal}";
        }

        private void UpdateMultiplierTextAndColor()
        {
            multiplierText.text = $"{viewModel.CurrentMultiplier}x";
            multiplierText.color = HudTextColor;
        }

        private void UpdatePointsText()
        {
            pointsText.text = $"+{viewModel.DisplayPoints} SCORE";
            pointsText.color = HudTextColor;
        }

        private void UpdateVisibility(bool visible, bool animate = true)
        {
            if (visible)
            {
                if (canvasGroup.alpha > 0f && fadeTween == null)
                {
                    return;
                }

                fadeTween?.Kill();
                if (animate)
                {
                    fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.3f);
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }

                StartPointsAnimation();
            }
            else
            {
                if (canvasGroup.alpha <= 0f && fadeTween == null)
                {
                    return;
                }

                StopPointsAnimation();
                StopMultiplierLoop();
                fadeTween?.Kill();
                if (animate)
                {
                    fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.5f);
                }
                else
                {
                    canvasGroup.alpha = 0f;
                }
            }
        }

        private void OnMultiplierIncreased()
        {
            multiplierPunchTween?.Kill();
            StopMultiplierLoop();

            multiplierText.transform.localScale = Vector3.one;

            float scaleMultiplier = 1f + (Mathf.Max(0, viewModel.CurrentMultiplier - 1) * punchScalePerTier);
            Vector3 actualPunchScale = multiplierPunchScale * scaleMultiplier;

            multiplierPunchTween = multiplierText.transform.DOPunchScale(actualPunchScale, punchDuration, 5, 1f)
                .OnComplete(() =>
                {
                    if (viewModel.CurrentMultiplier >= 5)
                    {
                        StartMultiplierLoop();
                    }
                });

            UpdateMultiplierTextAndColor();
            UpdatePointsText();
        }

        private void OnScoreBanked()
        {
            KillTweens();
            canvasGroup.alpha = 1f;
            UpdateMultiplierTextAndColor();
            UpdatePointsText();

            if (totalScoreText != null)
            {
                int startScore = 0;
                int.TryParse(totalScoreText.text, out startScore);
                int endScore = viewModel.TotalDriftScore;

                DOTween.To(() => startScore, x =>
                {
                    startScore = x;
                    totalScoreText.text = $"{startScore}";
                }, endScore, 0.5f).SetEase(Ease.OutQuad);

                totalScoreText.transform.DOKill(true);
                totalScoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 1f);
            }

            fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.5f).SetDelay(1f);
        }

        private void StartPointsAnimation()
        {
            if (pointsTween != null && pointsTween.IsActive())
            {
                return;
            }

            pointsText.transform.localScale = Vector3.one;
            pointsTween = pointsText.transform.DOScale(pointScalePingPong, pointPingPongDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPointsAnimation()
        {
            pointsTween?.Kill();
            pointsText.transform.localScale = Vector3.one;
        }

        private void StartMultiplierLoop()
        {
            if (multiplierLoopTween != null && multiplierLoopTween.IsActive())
            {
                return;
            }

            multiplierText.transform.localScale = Vector3.one;
            multiplierLoopTween = multiplierText.transform.DOScale(1.15f, 0.25f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopMultiplierLoop()
        {
            multiplierLoopTween?.Kill();
            multiplierText.transform.localScale = Vector3.one;
        }

        private void KillTweens()
        {
            multiplierPunchTween?.Kill();
            StopMultiplierLoop();
            pointsText.transform.DOKill();
            fadeTween?.Kill();
            pointsTween?.Kill();
        }
    }
}
