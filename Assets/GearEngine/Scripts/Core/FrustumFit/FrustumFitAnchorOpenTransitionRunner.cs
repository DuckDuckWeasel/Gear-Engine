using DG.Tweening;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Inspector-pluggable runner for <see cref="FrustumFitAnchorOpenTransition.Play(System.Collections.Generic.IReadOnlyList{FrustumFitAnchor},float,DG.Tweening.Ease)"/>.
    /// </summary>
    public sealed class FrustumFitAnchorOpenTransitionRunner : MonoBehaviour
    {
        [SerializeField]
        private FrustumFitAnchor[] anchors;

        [SerializeField]
        private float durationSeconds = 0.35f;

        [SerializeField]
        private Ease ease = Ease.InOutQuad;

        private Tween _activeTween;

        public void Play()
        {
            _activeTween?.Kill();
            _activeTween = FrustumFitAnchorOpenTransition.Play(anchors, durationSeconds, ease);
        }

        private void OnDisable()
        {
            _activeTween?.Kill();
            _activeTween = null;
        }
    }
}
