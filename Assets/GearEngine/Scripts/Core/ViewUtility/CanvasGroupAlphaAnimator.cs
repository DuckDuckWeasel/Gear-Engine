using UnityEngine;
using DG.Tweening;

namespace GearEngine.Core.ViewUtility
{
    /// <summary>
    /// Utility component to animate or instantly set a CanvasGroup's alpha.
    /// Exposes a float value setter for UnityEvents and AnimationEvents.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupAlphaAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Tween Settings")]
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private Ease defaultEase = Ease.Linear;

        private Tween currentTween;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Instantly sets the alpha of the CanvasGroup.
        /// Can be used by Animation Events or UnityEvents.
        /// </summary>
        public void SetAlpha(float value)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// Tweens the alpha to 1 using default duration and ease.
        /// </summary>
        public void FadeIn()
        {
            TweenAlpha(1f);
        }

        /// <summary>
        /// Tweens the alpha to 0 using default duration and ease.
        /// </summary>
        public void FadeOut()
        {
            TweenAlpha(0f);
        }

        /// <summary>
        /// Tweens the alpha to a specific target value.
        /// </summary>
        public void TweenAlpha(float targetAlpha)
        {
            if (canvasGroup == null) return;
            
            currentTween?.Kill();
            currentTween = canvasGroup.DOFade(Mathf.Clamp01(targetAlpha), defaultDuration).SetEase(defaultEase);
        }

        private void OnDestroy()
        {
            currentTween?.Kill();
        }
    }
}
