using System.Collections;
using UnityEngine;

namespace GearEngine.FrustumFit.Samples
{
    public enum FrustumFitSampleUpdateMode
    {
        Continuous,
        ApplyOnKey,
        TweenOnKey,
    }

    /// <summary>
    /// Switches the sample scene between continuous fitting, key-triggered <see cref="FrustumFitAnchor.Apply"/>,
    /// and key-triggered smoothing toward a freshly computed placement.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class FrustumFitSampleController : MonoBehaviour
    {
        [SerializeField]
        private FrustumFitSampleUpdateMode mode = FrustumFitSampleUpdateMode.Continuous;

        [SerializeField]
        private FrustumFitAnchor anchor;

        [SerializeField]
        private KeyCode triggerKey = KeyCode.Space;

        [SerializeField]
        private float tweenDuration = 0.35f;

        private void Awake()
        {
            ApplyModeToAnchor();
        }

        private void Start()
        {
            if (mode == FrustumFitSampleUpdateMode.TweenOnKey && anchor != null)
            {
                anchor.Apply();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            if (anchor == null)
            {
                return;
            }

            if (mode == FrustumFitSampleUpdateMode.ApplyOnKey && Input.GetKeyDown(triggerKey))
            {
                anchor.Apply();
            }

            if (mode == FrustumFitSampleUpdateMode.TweenOnKey && Input.GetKeyDown(triggerKey))
            {
                StopAllCoroutines();
                StartCoroutine(TweenToPlacement());
            }
        }

        private void ApplyModeToAnchor()
        {
            if (anchor == null)
            {
                return;
            }

            switch (mode)
            {
                case FrustumFitSampleUpdateMode.Continuous:
                    anchor.ConfigureAutoApply(true, true);
                    break;
                case FrustumFitSampleUpdateMode.ApplyOnKey:
                    anchor.ConfigureAutoApply(true, false);
                    break;
                case FrustumFitSampleUpdateMode.TweenOnKey:
                    anchor.ConfigureAutoApply(false, false);
                    break;
            }
        }

        private IEnumerator TweenToPlacement()
        {
            Transform target = anchor.TargetTransform;
            if (target == null || !anchor.TryComputePlacement(out FrustumFitAnchorPlacement end))
            {
                yield break;
            }

            Vector3 startPos = target.position;
            Vector3 startScale = target.localScale;
            Quaternion startRot = target.rotation;

            float duration = Mathf.Max(0.01f, tweenDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                u = u * u * (3f - 2f * u);

                target.position = Vector3.Lerp(startPos, end.WorldPosition, u);
                target.localScale = Vector3.Lerp(startScale, end.LocalScale, u);
                if (end.HasWorldRotation)
                {
                    target.rotation = Quaternion.Slerp(startRot, end.WorldRotation, u);
                }

                yield return null;
            }

            end.ApplyTo(target);
        }
    }
}
