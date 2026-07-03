using UnityEngine;
using TMPro;
using DG.Tweening;
using Scaffold.MVVM;
using System.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed class RaceDriftScoreView : ViewComponent<RaceDriftScoreViewModel>
    {
        [Header("References")]
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Juice Settings")]
        [SerializeField] private Vector3 multiplierPunchScale = new Vector3(0.5f, 0.5f, 0f);
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private float pointScalePingPong = 1.05f;
        [SerializeField] private float pointPingPongDuration = 0.2f;

        private Tween pointsTween;
        private Tween fadeTween;
        private Tween multiplierPunchTween;

        protected override void OnBind()
        {
            base.OnBind();
            
            viewModel.MultiplierIncreased += OnMultiplierIncreased;
            viewModel.ScoreBanked += OnScoreBanked;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            UpdateVisibility(false, false);
            multiplierText.text = $"{viewModel.CurrentMultiplier}x";
            pointsText.text = $"{viewModel.DisplayPoints}";
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
                pointsText.text = $"{viewModel.DisplayPoints}";
            }
            else if (e.PropertyName == nameof(viewModel.CurrentMultiplier))
            {
                multiplierText.text = $"{viewModel.CurrentMultiplier}x";
            }
        }

        private void UpdateVisibility(bool visible, bool animate = true)
        {
            if (visible)
            {
                if (canvasGroup.alpha > 0f && fadeTween == null) return;
                
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
                if (canvasGroup.alpha <= 0f && fadeTween == null) return;
                
                StopPointsAnimation();
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
            multiplierText.transform.localScale = Vector3.one;
            multiplierPunchTween = multiplierText.transform.DOPunchScale(multiplierPunchScale, punchDuration, 5, 1f);
            
            multiplierText.text = $"{viewModel.CurrentMultiplier}x";
            pointsText.text = $"{viewModel.DisplayPoints}";
        }

        private void OnScoreBanked()
        {
            KillTweens();
            canvasGroup.alpha = 1f;
            multiplierText.text = $"{viewModel.CurrentMultiplier}x";
            pointsText.text = $"{viewModel.DisplayPoints}";
            
            fadeTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, 0.5f).SetDelay(1f);
        }

        private void StartPointsAnimation()
        {
            if (pointsTween != null && pointsTween.IsActive()) return;
            
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

        private void KillTweens()
        {
            multiplierPunchTween?.Kill();
            pointsText.transform.DOKill();
            fadeTween?.Kill();
            pointsTween?.Kill();
        }
    }
}
