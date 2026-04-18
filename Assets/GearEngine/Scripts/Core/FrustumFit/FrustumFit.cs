using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Scales this GameObject's <see cref="Renderer"/> to a fraction of the <strong>full</strong> camera frustum at <see cref="depth"/>.
    /// For UI-anchored screen regions, use <see cref="FrustumFitAnchor"/> instead.
    /// </summary>
    public sealed class FrustumFit : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Camera used to measure the frustum. Defaults to Camera.main if left empty.")]
        [FormerlySerializedAs("_camera")]
        [SerializeField]
        private Camera frustumCamera;

        [Tooltip("World-space distance from the camera at which frustum dimensions are sampled. " +
                 "Set this to the object's Z distance from the camera. " +
                 "Has no effect on orthographic cameras.")]
        [FormerlySerializedAs("_depth")]
        [SerializeField]
        private float depth = 10f;

        [Header("Fill")]
        [Tooltip("Fraction of the frustum width the object should occupy. " +
                 "1 = full width, 0.5 = half width. Values > 1 intentionally overflow the screen.")]
        [FormerlySerializedAs("_fillX")]
        [SerializeField, Range(0f, 2f)]
        private float fillX = 1f;

        [Tooltip("Fraction of the frustum height the object should occupy.")]
        [FormerlySerializedAs("_fillY")]
        [SerializeField, Range(0f, 2f)]
        private float fillY = 1f;

        [Tooltip("How the object fills the configured target box.")]
        [FormerlySerializedAs("_mode")]
        [SerializeField]
        private FrustumFillMode fillMode = FrustumFillMode.Fit;

        [Header("Axes")]
        [Tooltip("Which two local axes map to screen horizontal and vertical. " +
                 "XY = Quad/sprite (default). XZ = Unity Plane rotated to face camera. YZ = rare custom meshes.")]
        [FormerlySerializedAs("_axes")]
        [SerializeField]
        private FrustumFitAxes fitAxes = FrustumFitAxes.XY;

        [Header("Update Timing")]
        [Tooltip("Call Apply() automatically on Start.")]
        [FormerlySerializedAs("_applyOnStart")]
        [SerializeField]
        private bool applyOnStart = true;

        [Tooltip("Call Apply() every LateUpdate. Use only when the camera, window size, " +
                 "or fill settings change at runtime. Disable and call Apply() externally " +
                 "from a resize coordinator for better performance.")]
        [FormerlySerializedAs("_applyEveryFrame")]
        [SerializeField]
        private bool applyEveryFrame = false;

        private Renderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<Renderer>();
            WarnIfMissingRenderer();
            ResolveFrustumCameraReference();
        }

        private void WarnIfMissingRenderer()
        {
            if (meshRenderer == null)
            {
                Debug.LogError($"[FrustumFit] No Renderer found on '{name}'. Attach FrustumFit to the GameObject that owns the Renderer, or move the Renderer to this GameObject.");
            }
        }

        private void ResolveFrustumCameraReference()
        {
            if (frustumCamera == null)
            {
                frustumCamera = Camera.main;
            }

            if (frustumCamera == null)
            {
                Debug.LogError($"[FrustumFit] No camera assigned and Camera.main is null on '{name}'. Assign a camera in the Inspector.");
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
                TryApplyFrustumScale();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FrustumFit] Apply failed on '{name}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TryApplyFrustumScale()
        {
            if (!HasCameraAndRendererForApply())
            {
                return;
            }

            FrustumBounds bounds = ReadBoundsFromCamera();
            ApplyScaleForBounds(bounds);
        }

        private bool HasCameraAndRendererForApply()
        {
            if (frustumCamera == null)
            {
                Debug.LogError($"[FrustumFit] Cannot Apply — camera is null on '{name}'.");
                return false;
            }

            if (meshRenderer == null)
            {
                Debug.LogError($"[FrustumFit] Cannot Apply — Renderer is null on '{name}'.");
                return false;
            }

            return true;
        }

        private FrustumBounds ReadBoundsFromCamera()
        {
            return FrustumFitMath.ComputeBounds(frustumCamera.orthographic, frustumCamera.orthographicSize, frustumCamera.fieldOfView, frustumCamera.aspect, depth);
        }

        private void ApplyScaleForBounds(FrustumBounds bounds)
        {
            Vector3 boundsSize = meshRenderer.localBounds.size;
            Vector2 meshSize = FrustumFitAxisMapping.ExtractAxesPair(boundsSize, fitAxes);
            Vector2 parentScale = GetParentLossyScalePair(fitAxes);
            Vector2 localScale2D = FrustumFitMath.ComputeLocalScale(bounds, fillX, fillY, fillMode, meshSize, parentScale);
            FrustumFitAxisMapping.WriteLocalScaleAxes(transform, localScale2D, fitAxes);
        }

        private Vector2 GetParentLossyScalePair(FrustumFitAxes axes)
        {
            Vector3 ps = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            return FrustumFitAxisMapping.ExtractAxesPair(ps, axes);
        }
    }
}
