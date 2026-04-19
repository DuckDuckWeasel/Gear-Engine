using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// DOTween-driven transition of frustum-fit targets to their computed placement. Snap final pose with <see cref="FrustumFitAnchorPlacement.ApplyTo"/>.
    /// </summary>
    public static class FrustumFitAnchorOpenTransition
    {
        private const float MinimumDurationSeconds = 0.01f;

        /// <summary>
        /// Tweens the anchor's target transform toward a freshly computed placement. Returns <c>null</c> if the anchor
        /// is invalid or placement cannot be computed.
        /// </summary>
        /// <param name="anchor">Frustum-fit source; uses <see cref="FrustumFitAnchor.TargetTransform"/>.</param>
        /// <param name="durationSeconds">Tween length in seconds.</param>
        /// <param name="ease">Easing applied to position, scale, and rotation tweens.</param>
        public static Tween Play(FrustumFitAnchor anchor, float durationSeconds, Ease ease = Ease.InOutQuad)
        {
            if (anchor == null || anchor.TargetTransform == null)
            {
                return null;
            }

            if (!anchor.TryComputePlacement(out FrustumFitAnchorPlacement end))
            {
                return null;
            }

            Transform t = anchor.TargetTransform;
            float d = Mathf.Max(MinimumDurationSeconds, durationSeconds);
            Sequence seq = DOTween.Sequence();
            JoinPlacementTweens(seq, t, end, d, ease);
            seq.OnComplete(() => end.ApplyTo(t));
            return seq;
        }

        /// <summary>
        /// Tweens all valid anchors in parallel on a shared timeline. Returns <c>null</c> if the list is empty or no placement could be computed.
        /// </summary>
        /// <param name="anchors">Anchors to drive (null entries are skipped).</param>
        /// <param name="durationSeconds">Tween length in seconds.</param>
        /// <param name="ease">Easing applied to position, scale, and rotation tweens.</param>
        /// <param name="onAfterSnap">Invoked after all <see cref="FrustumFitAnchorPlacement.ApplyTo"/> calls when the tween completes.</param>
        public static Tween Play(
            IReadOnlyList<FrustumFitAnchor> anchors,
            float durationSeconds,
            Ease ease = Ease.InOutQuad,
            Action onAfterSnap = null)
        {
            if (anchors == null || anchors.Count == 0)
            {
                return null;
            }

            var pairs = new List<(Transform Target, FrustumFitAnchorPlacement Placement)>();
            for (int i = 0; i < anchors.Count; i++)
            {
                FrustumFitAnchor anchor = anchors[i];
                if (anchor == null || anchor.TargetTransform == null)
                {
                    continue;
                }

                if (!anchor.TryComputePlacement(out FrustumFitAnchorPlacement end))
                {
                    continue;
                }

                pairs.Add((anchor.TargetTransform, end));
            }

            if (pairs.Count == 0)
            {
                return null;
            }

            float d = Mathf.Max(MinimumDurationSeconds, durationSeconds);
            Sequence main = DOTween.Sequence();
            for (int i = 0; i < pairs.Count; i++)
            {
                (Transform target, FrustumFitAnchorPlacement placement) = pairs[i];
                Sequence item = DOTween.Sequence();
                JoinPlacementTweens(item, target, placement, d, ease);
                main.Join(item);
            }

            main.OnComplete(() =>
            {
                for (int i = 0; i < pairs.Count; i++)
                {
                    (Transform target, FrustumFitAnchorPlacement placement) = pairs[i];
                    placement.ApplyTo(target);
                }

                onAfterSnap?.Invoke();
            });

            return main;
        }

        /// <summary>
        /// After one frame (and <see cref="Canvas.ForceUpdateCanvases"/>), runs <see cref="Play(IReadOnlyList{FrustumFitAnchor},float,Ease,Action)"/>
        /// with <see cref="Ease.InOutQuad"/>. Temporarily disables continuous <see cref="FrustumFitAnchor"/> auto-apply so tweens are not overwritten,
        /// then restores the anchors' original <c>applyOnStart</c> / <c>applyEveryFrame</c> when the tween finishes.
        /// </summary>
        /// <remarks>This overload avoids a <see cref="Ease"/> parameter so callers (e.g. other assemblies) do not need a reference to DOTween.dll.</remarks>
        /// <param name="host">MonoBehaviour used to run the layout coroutine.</param>
        /// <param name="anchors">Anchors to tween; null or all-null entries are ignored (no coroutine started if none).</param>
        /// <param name="onComplete">Invoked once after layout prep and after the tween snaps (or immediately if no tween runs). Restore of anchor auto-apply runs before this.</param>
        public static void PlayAfterCanvasLayout(MonoBehaviour host, IReadOnlyList<FrustumFitAnchor> anchors, float durationSeconds, Action onComplete = null)
        {
            if (host == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (!HasAnyAnchor(anchors))
            {
                onComplete?.Invoke();
                return;
            }

            host.StartCoroutine(PlayAfterCanvasLayoutRoutine(
                anchors,
                durationSeconds,
                Ease.InOutQuad,
                suppressContinuousFitDuringTween: true,
                restoreApplyEveryFrameAfter: true,
                onComplete));
        }

        /// <summary>
        /// Same as <see cref="PlayAfterCanvasLayout(MonoBehaviour,IReadOnlyList{FrustumFitAnchor},float,Action)"/> for inline anchor arguments.
        /// </summary>
        public static void PlayAfterCanvasLayout(MonoBehaviour host, float durationSeconds, params FrustumFitAnchor[] anchors)
        {
            if (anchors == null || anchors.Length == 0)
            {
                return;
            }

            PlayAfterCanvasLayout(host, new List<FrustumFitAnchor>(anchors), durationSeconds, onComplete: null);
        }

        private static bool HasAnyAnchor(IReadOnlyList<FrustumFitAnchor> anchors)
        {
            if (anchors == null || anchors.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerator PlayAfterCanvasLayoutRoutine(
            IReadOnlyList<FrustumFitAnchor> anchors,
            float durationSeconds,
            Ease ease,
            bool suppressContinuousFitDuringTween,
            bool restoreApplyEveryFrameAfter,
            Action onComplete)
        {
            List<FrustumFitAnchorAutoApplySnapshot> snapshots = suppressContinuousFitDuringTween
                ? CaptureAutoApplySnapshots(anchors)
                : null;

            if (suppressContinuousFitDuringTween)
            {
                SuppressContinuousFit(anchors);
            }

            Canvas.ForceUpdateCanvases();
            yield return null;

            bool shouldRestore = suppressContinuousFitDuringTween && restoreApplyEveryFrameAfter;
            Action restore = shouldRestore && snapshots != null ? () => RestoreAutoApplySnapshots(anchors, snapshots) : null;
            void Combined()
            {
                restore?.Invoke();
                onComplete?.Invoke();
            }

            Tween tween = Play(anchors, durationSeconds, ease, Combined);
            if (tween == null)
            {
                Combined();
            }
        }

        private static List<FrustumFitAnchorAutoApplySnapshot> CaptureAutoApplySnapshots(IReadOnlyList<FrustumFitAnchor> anchors)
        {
            var list = new List<FrustumFitAnchorAutoApplySnapshot>(anchors.Count);
            for (int i = 0; i < anchors.Count; i++)
            {
                FrustumFitAnchor anchor = anchors[i];
                list.Add(anchor != null ? FrustumFitAnchorAutoApplySnapshot.Capture(anchor) : default);
            }

            return list;
        }

        private static void SuppressContinuousFit(IReadOnlyList<FrustumFitAnchor> anchors)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                FrustumFitAnchor anchor = anchors[i];
                if (anchor != null)
                {
                    anchor.ConfigureAutoApply(false, false);
                }
            }
        }

        private static void RestoreAutoApplySnapshots(IReadOnlyList<FrustumFitAnchor> anchors, List<FrustumFitAnchorAutoApplySnapshot> snapshots)
        {
            int n = Mathf.Min(anchors.Count, snapshots.Count);
            for (int i = 0; i < n; i++)
            {
                FrustumFitAnchor anchor = anchors[i];
                if (anchor != null)
                {
                    snapshots[i].RestoreTo(anchor);
                }
            }
        }

        private readonly struct FrustumFitAnchorAutoApplySnapshot
        {
            private readonly bool applyOnStart;
            private readonly bool applyEveryFrame;

            public static FrustumFitAnchorAutoApplySnapshot Capture(FrustumFitAnchor anchor)
            {
                return new FrustumFitAnchorAutoApplySnapshot(anchor.ApplyOnStart, anchor.ApplyEveryFrame);
            }

            public void RestoreTo(FrustumFitAnchor anchor)
            {
                if (anchor != null)
                {
                    anchor.ConfigureAutoApply(applyOnStart, applyEveryFrame);
                }
            }

            private FrustumFitAnchorAutoApplySnapshot(bool applyOnStartValue, bool applyEveryFrameValue)
            {
                applyOnStart = applyOnStartValue;
                applyEveryFrame = applyEveryFrameValue;
            }
        }

        /// <summary>
        /// Convenience overload: same timeline for multiple anchors passed inline.
        /// </summary>
        public static Tween Play(float durationSeconds, Ease ease, params FrustumFitAnchor[] anchors)
        {
            if (anchors == null || anchors.Length == 0)
            {
                return null;
            }

            return Play(new List<FrustumFitAnchor>(anchors), durationSeconds, ease);
        }

        private static void JoinPlacementTweens(Sequence seq, Transform target, FrustumFitAnchorPlacement end, float duration, Ease ease)
        {
            seq.Join(target.DOMove(end.WorldPosition, duration).SetEase(ease));
            seq.Join(target.DOScale(end.LocalScale, duration).SetEase(ease));
            if (end.HasWorldRotation)
            {
                seq.Join(target.DORotateQuaternion(end.WorldRotation, duration).SetEase(ease));
            }
        }
    }
}
