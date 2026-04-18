using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// sample: UI-anchored frustum fitting; Canvas from source rect parents; Renderer from target transform parents or children.
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

            Renderer renderer = FrustumFitTargetRenderer.FromTargetTransform(targetTransform);
            Vector3 baseline = targetTransform != null ? targetTransform.localScale : Vector3.one;
            return FrustumFitPlacementFactory.TryCreate(sourceRect, canvas, worldCamera, depth, targetTransform, renderer, fillMode, fitAxes, rotationMode, baseline, out placement);
        }

        private void LogApplySkipped()
        {
            if (LogIfSourceRectMissing()) return;
            if (LogIfCanvasMissing()) return;
            if (LogIfCameraMissing()) return;
            if (LogIfTargetMissing()) return;
            if (LogIfRendererMissing()) return;
            if (LogIfDepthInvalid()) return;
            if (LogIfMeshExtentsInvalid()) return;
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

        private bool LogIfRendererMissing()
        {
            if (FrustumFitTargetRenderer.FromTargetTransform(targetTransform) != null)
            {
                return false;
            }

            Debug.LogError($"[FrustumFitAnchor] No Renderer on target ancestors or children on '{name}'. Assign targetTransform to a hierarchy that includes a Renderer.");
            return true;
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

        private bool LogIfMeshExtentsInvalid()
        {
            Renderer renderer = FrustumFitTargetRenderer.FromTargetTransform(targetTransform);
            Vector2 meshSize = FrustumFitAxisMapping.ExtractAxesPair(renderer.localBounds.size, fitAxes);
            if (meshSize.x > 0f && meshSize.y > 0f)
            {
                return false;
            }

            Debug.LogError(
                $"[FrustumFitAnchor] Renderer local bounds have no extent on fit axes {fitAxes} on '{name}' (size={meshSize}). " +
                "Use Frustum Fit Axes that match your mesh (e.g. XZ for a ground/track plane).");
            return true;
        }
    }

    public static class FrustumFitTargetRenderer
    {
        public static Renderer FromTargetTransform(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return null;
            }

            Renderer renderer = targetTransform.GetComponentInParent<Renderer>();
            return renderer != null ? renderer : targetTransform.GetComponentInChildren<Renderer>(true);
        }
    }

    public static class FrustumFitPlacementFactory
    {
        public static bool TryCreate(RectTransform sourceRect, Canvas sourceCanvas, Camera worldCamera, float depth, Transform targetTransform, Renderer targetRenderer, FrustumFillMode fillMode, FrustumFitAxes fitAxes, FrustumFitAnchorRotationMode rotationMode, Vector3 baselineLocalScale, out FrustumFitAnchorPlacement placement)
        {
            placement = default;
            if (!HasValidCoreInputs(sourceRect, worldCamera, targetTransform, targetRenderer) || !HasValidDepth(depth, worldCamera.orthographic))
            {
                return false;
            }

            if (!TryReadViewport(sourceRect, sourceCanvas, worldCamera, out Vector2 vpSize, out Vector2 vpCenter))
            {
                return false;
            }

            return TryBuildPlacement(worldCamera, depth, targetTransform, targetRenderer, fillMode, fitAxes, rotationMode, baselineLocalScale, vpSize, vpCenter, out placement);
        }

        private static bool HasValidCoreInputs(RectTransform sourceRect, Camera worldCamera, Transform targetTransform, Renderer targetRenderer)
        {
            return sourceRect != null && worldCamera != null && targetTransform != null && targetRenderer != null;
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

        private static bool TryBuildPlacement(Camera worldCamera, float depth, Transform targetTransform, Renderer targetRenderer, FrustumFillMode fillMode, FrustumFitAxes fitAxes, FrustumFitAnchorRotationMode rotationMode, Vector3 baselineLocalScale, Vector2 vpSize, Vector2 vpCenter, out FrustumFitAnchorPlacement placement)
        {
            FrustumBounds bounds = FrustumFitMath.ComputeBounds(worldCamera.orthographic, worldCamera.orthographicSize, worldCamera.fieldOfView, worldCamera.aspect, depth);
            Vector2 targetWorldSize = FrustumFitMath.ComputeTargetWorldSize(bounds, vpSize.x, vpSize.y);
            Vector3 worldCenter = worldCamera.ViewportToWorldPoint(new Vector3(vpCenter.x, vpCenter.y, depth));
            if (!TryMergeLocalScale(targetTransform, targetRenderer, targetWorldSize, fillMode, fitAxes, baselineLocalScale, out Vector3 localScale))
            {
                placement = default;
                return false;
            }

            bool hasRotation = rotationMode == FrustumFitAnchorRotationMode.MatchCameraRotation;
            Quaternion worldRotation = hasRotation ? worldCamera.transform.rotation : default;
            placement = new FrustumFitAnchorPlacement(worldCenter, localScale, hasRotation, worldRotation);
            return true;
        }

        private static bool TryMergeLocalScale(Transform targetTransform, Renderer targetRenderer, Vector2 targetWorldSize, FrustumFillMode fillMode, FrustumFitAxes fitAxes, Vector3 baselineLocalScale, out Vector3 localScale)
        {
            Vector3 boundsSize = targetRenderer.localBounds.size;
            Vector2 meshSize = FrustumFitAxisMapping.ExtractAxesPair(boundsSize, fitAxes);
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
