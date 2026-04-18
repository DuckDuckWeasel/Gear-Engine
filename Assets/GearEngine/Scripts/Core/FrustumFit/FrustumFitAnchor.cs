using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// UI-anchored frustum fitting: scales and positions a world-space target so it
    /// visually fills the screen region defined by a <see cref="RectTransform"/>.
    /// </summary>
    public sealed class FrustumFitAnchor : MonoBehaviour
    {
        public Transform TargetTransform => targetTransform;

        [Header("Source (UI)")]
        [Tooltip("Rect that defines the on-screen region. Defaults to this GameObject's RectTransform if empty.")]
        [SerializeField]
        private RectTransform sourceRect;

        [Header("Target (World)")]
        [Tooltip("Camera used for viewport and world placement. Defaults to Camera.main if empty.")]
        [SerializeField]
        private Camera worldCamera;

        [Tooltip("World-space distance from the camera for ViewportToWorldPoint (perspective) and frustum sizing.")]
        [SerializeField]
        private float depth = 10f;

        [SerializeField]
        private Transform targetTransform;

        [Tooltip("How the target's visual extent is measured.\n" +
                 "• DirectRenderer — reads the Renderer on targetTransform itself; fails if none.\n" +
                 "• CombineChildBounds — encapsulates all child Renderer world bounds; good for " +
                 "logical roots whose size is defined by many children (e.g. a grid of tiles).")]
        [SerializeField]
        private FrustumFitBoundsMode boundsMode = FrustumFitBoundsMode.DirectRenderer;

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

        public void SetWorldCamera(Camera camera)
        {
            worldCamera = camera;
        }

        /// <summary>Serialized auto-apply on <c>Start()</c>; exposed for snapshot/restore around transitions.</summary>
        public bool ApplyOnStart => applyOnStart;

        /// <summary>Serialized continuous apply in <c>LateUpdate()</c>; exposed for snapshot/restore around transitions.</summary>
        public bool ApplyEveryFrame => applyEveryFrame;

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

        public void Apply()
        {
            try
            {
                if (!TryComputePlacement(out FrustumFitAnchorPlacement placement))
                {
                    LogApplySkipped();
                    return;
                }

                placement.ApplyTo(targetTransform);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FrustumFitAnchor] Apply failed on '{name}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        public bool TryComputePlacement(out FrustumFitAnchorPlacement placement)
        {
            placement = default;
            if (sourceRect == null)
            {
                return false;
            }

            Canvas canvas = RectTransformScreenBoxUtility.ResolveCanvas(sourceRect, null);
            if (canvas == null)
            {
                return false;
            }

            if (!FrustumFitBoundsResolver.TryResolve(boundsMode, targetTransform, out Vector3 effectiveMeshSize))
            {
                return false;
            }

            Vector3 baseline = targetTransform != null ? targetTransform.localScale : Vector3.one;
            return FrustumFitPlacementFactory.TryCreate(sourceRect, canvas, worldCamera, depth, targetTransform, effectiveMeshSize, fillMode, fitAxes, rotationMode, baseline, out placement);
        }

        private void LogApplySkipped()
        {
            if (LogIfSourceRectMissing()) return;
            if (LogIfCanvasMissing()) return;
            if (LogIfCameraMissing()) return;
            if (LogIfTargetMissing()) return;
            if (LogIfBoundsInvalid()) return;
            if (LogIfDepthInvalid()) return;
            Debug.LogError($"[FrustumFitAnchor] Could not compute placement on '{name}' (degenerate viewport or invalid inputs).");
        }

        private bool LogIfSourceRectMissing()
        {
            if (sourceRect != null)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] sourceRect is null on '{name}'. Assign a RectTransform or place this component on a UI element with a RectTransform.");
            return true;
        }

        private bool LogIfCanvasMissing()
        {
            if (RectTransformScreenBoxUtility.ResolveCanvas(sourceRect, null) != null)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] No Canvas found in parents of sourceRect on '{name}'.");
            return true;
        }

        private bool LogIfCameraMissing()
        {
            if (worldCamera != null)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] worldCamera is null on '{name}'. Assign a camera or set Camera.main.");
            return true;
        }

        private bool LogIfTargetMissing()
        {
            if (targetTransform != null)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] targetTransform is null on '{name}'.");
            return true;
        }

        private bool LogIfBoundsInvalid()
        {
            if (!FrustumFitBoundsResolver.TryResolve(boundsMode, targetTransform, out Vector3 effectiveMeshSize))
            {
                Debug.LogError($"[FrustumFitAnchor] '{name}': boundsMode={boundsMode} found no valid Renderer on '{targetTransform?.name}'. " +
                               "Switch to CombineChildBounds or add a Renderer to the target.");
                return true;
            }

            Vector2 meshSize2D = FrustumFitAxisMapping.ExtractAxesPair(effectiveMeshSize, fitAxes);
            if (meshSize2D.x <= 0f || meshSize2D.y <= 0f)
            {
                Debug.LogError($"[FrustumFitAnchor] '{name}': effective bounds have no extent on fit axes {fitAxes} (size={meshSize2D}). " +
                               "Adjust fitAxes to match your mesh (e.g. XZ for a ground/track plane).");
                return true;
            }

            return false;
        }

        private bool LogIfDepthInvalid()
        {
            if (depth > 0f || worldCamera.orthographic)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] depth must be positive for perspective cameras on '{name}'.");
            return true;
        }
    }

    public static class FrustumFitPlacementFactory
    {
        /// <summary>
        /// Computes a <see cref="FrustumFitAnchorPlacement"/> that scales and positions
        /// <paramref name="targetTransform"/> so it fills the screen region described by
        /// <paramref name="sourceRect"/>.
        /// </summary>
        /// <param name="effectiveMeshSize">
        /// The target mesh extent per unit of <paramref name="targetTransform"/> lossyScale,
        /// as produced by <see cref="FrustumFitBoundsResolver.TryResolve"/>.
        /// </param>
        public static bool TryCreate(
            RectTransform sourceRect,
            Canvas sourceCanvas,
            Camera worldCamera,
            float depth,
            Transform targetTransform,
            Vector3 effectiveMeshSize,
            FrustumFillMode fillMode,
            FrustumFitAxes fitAxes,
            FrustumFitAnchorRotationMode rotationMode,
            Vector3 baselineLocalScale,
            out FrustumFitAnchorPlacement placement)
        {
            placement = default;
            if (!HasValidCoreInputs(sourceRect, worldCamera, targetTransform) || !HasValidDepth(depth, worldCamera.orthographic))
            {
                return false;
            }

            if (!TryReadViewport(sourceRect, sourceCanvas, worldCamera, out Vector2 vpSize, out Vector2 vpCenter))
            {
                return false;
            }

            return TryBuildPlacement(worldCamera, depth, targetTransform, effectiveMeshSize, fillMode, fitAxes, rotationMode, baselineLocalScale, vpSize, vpCenter, out placement);
        }

        private static bool HasValidCoreInputs(RectTransform sourceRect, Camera worldCamera, Transform targetTransform)
        {
            return sourceRect != null && worldCamera != null && targetTransform != null;
        }

        private static bool HasValidDepth(float depth, bool orthographic)
        {
            return depth > 0f || orthographic;
        }

        private static bool TryReadViewport(RectTransform sourceRect, Canvas sourceCanvas, Camera worldCamera, out Vector2 vpSize, out Vector2 vpCenter)
        {
            RectTransformScreenBoxUtility.GetViewportBounds(sourceRect, sourceCanvas, worldCamera, out Vector2 vpMin, out Vector2 vpMax);
            RectTransformScreenBoxUtility.GetViewportSizeAndCenter(vpMin, vpMax, out vpSize, out vpCenter);
            return vpSize.x > 0f && vpSize.y > 0f;
        }

        private static bool TryBuildPlacement(
            Camera worldCamera,
            float depth,
            Transform targetTransform,
            Vector3 effectiveMeshSize,
            FrustumFillMode fillMode,
            FrustumFitAxes fitAxes,
            FrustumFitAnchorRotationMode rotationMode,
            Vector3 baselineLocalScale,
            Vector2 vpSize,
            Vector2 vpCenter,
            out FrustumFitAnchorPlacement placement)
        {
            FrustumBounds bounds = FrustumFitMath.ComputeBounds(worldCamera.orthographic, worldCamera.orthographicSize, worldCamera.fieldOfView, worldCamera.aspect, depth);
            Vector2 targetWorldSize = FrustumFitMath.ComputeTargetWorldSize(bounds, vpSize.x, vpSize.y);
            Vector3 worldCenter = worldCamera.ViewportToWorldPoint(new Vector3(vpCenter.x, vpCenter.y, depth));

            if (!TryMergeLocalScale(targetTransform, effectiveMeshSize, targetWorldSize, fillMode, fitAxes, baselineLocalScale, out Vector3 localScale))
            {
                placement = default;
                return false;
            }

            bool hasRotation = rotationMode == FrustumFitAnchorRotationMode.MatchCameraRotation;
            Quaternion worldRotation = hasRotation ? worldCamera.transform.rotation : default;
            placement = new FrustumFitAnchorPlacement(worldCenter, localScale, hasRotation, worldRotation);
            return true;
        }

        private static bool TryMergeLocalScale(
            Transform targetTransform,
            Vector3 effectiveMeshSize,
            Vector2 targetWorldSize,
            FrustumFillMode fillMode,
            FrustumFitAxes fitAxes,
            Vector3 baselineLocalScale,
            out Vector3 localScale)
        {
            Vector2 meshSize = FrustumFitAxisMapping.ExtractAxesPair(effectiveMeshSize, fitAxes);
            if (meshSize.x <= 0f || meshSize.y <= 0f)
            {
                localScale = default;
                return false;
            }

            Vector3 ps = targetTransform.parent != null ? targetTransform.parent.lossyScale : Vector3.one;
            Vector2 parentScale = FrustumFitAxisMapping.ExtractAxesPair(ps, fitAxes);
            Vector2 localScale2D = FrustumFitMath.ComputeLocalScaleForTargetSize(targetWorldSize, fillMode, meshSize, parentScale);
            localScale = FrustumFitAxisMapping.MergeLocalScaleAxes(baselineLocalScale, localScale2D, fitAxes);
            return true;
        }
    }
}
