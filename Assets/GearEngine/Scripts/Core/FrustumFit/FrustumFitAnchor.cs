using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// UI-driven bridge: reads a resolved <see cref="RectTransform"/> screen box and positions/scales a world-space target to match it at <see cref="depth"/>.
    /// Use <see cref="TryComputePlacement(out FrustumFitAnchorPlacement)"/> to get values for tweens without applying.
    /// </summary>
    public sealed class FrustumFitAnchor : MonoBehaviour
    {
        [Header("Source (UI)")]
        [Tooltip("Rect that defines the on-screen region. Defaults to this GameObject's RectTransform if empty.")]
        [SerializeField]
        private RectTransform sourceRect;

        [Tooltip("Canvas for UI → screen projection. Defaults to a parent Canvas if empty.")]
        [SerializeField]
        private Canvas sourceCanvas;

        [Header("Target (World)")]
        [Tooltip("Camera used for viewport and world placement. Defaults to Camera.main if empty.")]
        [SerializeField]
        private Camera worldCamera;

        [Tooltip("World-space distance from the camera for ViewportToWorldPoint (perspective) and frustum sizing.")]
        [SerializeField]
        private float depth = 10f;

        [SerializeField]
        private Transform targetTransform;

        [SerializeField]
        private Renderer targetRenderer;

        [Header("Fit")]
        [SerializeField]
        private FrustumFillMode fillMode = FrustumFillMode.Fit;

        [SerializeField]
        private FrustumFitAxes fitAxes = FrustumFitAxes.XY;

        [Tooltip("When applying or computing placement, whether to output a world rotation (e.g. match camera for screen-facing objects).")]
        [SerializeField]
        private FrustumFitAnchorRotationMode rotationMode = FrustumFitAnchorRotationMode.PreserveTarget;

        [Header("Update Timing")]
        [SerializeField]
        private bool applyOnStart = true;

        [SerializeField]
        private bool applyEveryFrame;

        public Transform TargetTransform => targetTransform;

        /// <summary>Reconfigures automatic apply timing (same fields as the Inspector). Runs before <see cref="Start"/> if called from <see cref="Awake"/> on a component with earlier execution order.</summary>
        public void ConfigureAutoApply(bool applyOnStartValue, bool applyEveryFrameValue)
        {
            applyOnStart = applyOnStartValue;
            applyEveryFrame = applyEveryFrameValue;
        }

        private void Awake()
        {
            if (sourceRect == null)
            {
                sourceRect = GetComponent<RectTransform>();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void Start()
        {
            if (applyOnStart)
            {
                Apply();
            }
        }

        private void LateUpdate()
        {
            if (applyEveryFrame)
            {
                Apply();
            }
        }

        /// <summary>
        /// Computes world position, local scale, and optional world rotation without modifying the target.
        /// The third local scale axis is taken from <paramref name="baselineLocalScale"/>.
        /// </summary>
        public static bool TryComputePlacement(
            RectTransform sourceRect,
            Canvas sourceCanvas,
            Camera worldCamera,
            float depth,
            Transform targetTransform,
            Renderer targetRenderer,
            FrustumFillMode fillMode,
            FrustumFitAxes fitAxes,
            FrustumFitAnchorRotationMode rotationMode,
            Vector3 baselineLocalScale,
            out FrustumFitAnchorPlacement placement)
        {
            placement = default;

            if (sourceRect == null || worldCamera == null || targetTransform == null || targetRenderer == null)
            {
                return false;
            }

            if (depth <= 0f && !worldCamera.orthographic)
            {
                return false;
            }

            RectTransformScreenBoxUtility.GetViewportBounds(sourceRect, sourceCanvas, worldCamera, out Vector2 vpMin, out Vector2 vpMax);
            RectTransformScreenBoxUtility.GetViewportSizeAndCenter(vpMin, vpMax, out Vector2 vpSize, out Vector2 vpCenter);

            if (vpSize.x <= 0f || vpSize.y <= 0f)
            {
                return false;
            }

            FrustumBounds bounds = FrustumFitMath.ComputeBounds(
                worldCamera.orthographic,
                worldCamera.orthographicSize,
                worldCamera.fieldOfView,
                worldCamera.aspect,
                depth);

            Vector2 targetWorldSize = FrustumFitMath.ComputeTargetWorldSize(bounds, vpSize.x, vpSize.y);
            Vector3 worldCenter = worldCamera.ViewportToWorldPoint(new Vector3(vpCenter.x, vpCenter.y, depth));

            Vector3 boundsSize = targetRenderer.localBounds.size;
            Vector2 meshSize = FrustumFitAxisMapping.ExtractAxesPair(boundsSize, fitAxes);
            Vector3 ps = targetTransform.parent != null ? targetTransform.parent.lossyScale : Vector3.one;
            Vector2 parentScale = FrustumFitAxisMapping.ExtractAxesPair(ps, fitAxes);
            Vector2 localScale2D = FrustumFitMath.ComputeLocalScaleForTargetSize(targetWorldSize, fillMode, meshSize, parentScale);
            Vector3 localScale = FrustumFitAxisMapping.MergeLocalScaleAxes(baselineLocalScale, localScale2D, fitAxes);

            bool hasRotation = rotationMode == FrustumFitAnchorRotationMode.MatchCameraRotation;
            Quaternion worldRotation = hasRotation ? worldCamera.transform.rotation : default;
            placement = new FrustumFitAnchorPlacement(worldCenter, localScale, hasRotation, worldRotation);
            return true;
        }

        /// <inheritdoc cref="TryComputePlacement(RectTransform, Canvas, Camera, float, Transform, Renderer, FrustumFillMode, FrustumFitAxes, FrustumFitAnchorRotationMode, Vector3, out FrustumFitAnchorPlacement)"/>
        public bool TryComputePlacement(out FrustumFitAnchorPlacement placement)
        {
            return TryComputePlacement(
                sourceRect,
                sourceCanvas,
                worldCamera,
                depth,
                targetTransform,
                targetRenderer,
                fillMode,
                fitAxes,
                rotationMode,
                targetTransform != null ? targetTransform.localScale : Vector3.one,
                out placement);
        }

        public void Apply()
        {
            try
            {
                if (!TryApplyFromCompute())
                {
                    LogApplySkipped();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FrustumFitAnchor] Apply failed on '{name}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool TryApplyFromCompute()
        {
            if (!TryComputePlacement(out FrustumFitAnchorPlacement placement))
            {
                return false;
            }

            ApplyPlacement(targetTransform, placement);
            return true;
        }

        /// <summary>
        /// Writes a computed placement to a transform (position, localScale, optional world rotation).
        /// </summary>
        public static void ApplyPlacement(Transform targetTransform, FrustumFitAnchorPlacement placement)
        {
            if (targetTransform == null)
            {
                throw new ArgumentNullException(nameof(targetTransform));
            }

            targetTransform.position = placement.WorldPosition;
            targetTransform.localScale = placement.LocalScale;
            if (placement.HasWorldRotation)
            {
                targetTransform.rotation = placement.WorldRotation;
            }
        }

        private void LogApplySkipped()
        {
            if (sourceRect == null)
            {
                Debug.LogError($"[FrustumFitAnchor] sourceRect is null on '{name}'. Assign a RectTransform or place this component on a UI element with a RectTransform.");
                return;
            }

            if (worldCamera == null)
            {
                Debug.LogError($"[FrustumFitAnchor] worldCamera is null on '{name}'. Assign a camera or set Camera.main.");
                return;
            }

            if (targetTransform == null)
            {
                Debug.LogError($"[FrustumFitAnchor] targetTransform is null on '{name}'.");
                return;
            }

            if (targetRenderer == null)
            {
                Debug.LogError($"[FrustumFitAnchor] targetRenderer is null on '{name}'.");
                return;
            }

            if (depth <= 0f && !worldCamera.orthographic)
            {
                Debug.LogError($"[FrustumFitAnchor] depth must be positive for perspective cameras on '{name}'.");
                return;
            }

            Debug.LogError($"[FrustumFitAnchor] Could not compute placement on '{name}' (degenerate viewport or invalid inputs).");
        }
    }
}
