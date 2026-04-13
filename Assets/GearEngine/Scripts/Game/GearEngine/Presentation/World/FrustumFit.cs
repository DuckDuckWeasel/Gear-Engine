using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.GearEngine.Presentation.World
{
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
            Vector2 meshSize = ExtractAxesPair(boundsSize, fitAxes);
            Vector2 parentScale = GetParentLossyScalePair(fitAxes);
            Vector2 localScale2D = FrustumFitMath.ComputeLocalScale(bounds, fillX, fillY, fillMode, meshSize, parentScale);
            WriteLocalScaleAxes(localScale2D, fitAxes);
        }

        private Vector2 GetParentLossyScalePair(FrustumFitAxes axes)
        {
            Vector3 ps = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            return ExtractAxesPair(ps, axes);
        }

        private Vector2 ExtractAxesPair(Vector3 v, FrustumFitAxes axes)
        {
            return axes switch
            {
                FrustumFitAxes.XY => new Vector2(v.x, v.y),
                FrustumFitAxes.XZ => new Vector2(v.x, v.z),
                FrustumFitAxes.YZ => new Vector2(v.y, v.z),
                _ => throw new ArgumentOutOfRangeException(nameof(axes), axes, null),
            };
        }

        private void WriteLocalScaleAxes(Vector2 scale2D, FrustumFitAxes axes)
        {
            Vector3 ls = transform.localScale;
            BuildLocalScaleComponents(ref ls, scale2D, axes);
            transform.localScale = ls;
        }

        private void BuildLocalScaleComponents(ref Vector3 ls, Vector2 scale2D, FrustumFitAxes axes)
        {
            if (!TryWriteAxisPairToLocalScale(ref ls, scale2D, axes))
            {
                throw new ArgumentOutOfRangeException(nameof(axes), axes, null);
            }
        }

        private bool TryWriteAxisPairToLocalScale(ref Vector3 ls, Vector2 scale2D, FrustumFitAxes axes)
        {
            if (axes == FrustumFitAxes.XY)
            {
                BuildXyLocalScale(ref ls, scale2D);
                return true;
            }

            return TryWriteNonXyAxes(ref ls, scale2D, axes);
        }

        private void BuildXyLocalScale(ref Vector3 ls, Vector2 scale2D)
        {
            ls.x = scale2D.x;
            ls.y = scale2D.y;
        }

        private bool TryWriteNonXyAxes(ref Vector3 ls, Vector2 scale2D, FrustumFitAxes axes)
        {
            if (axes == FrustumFitAxes.XZ)
            {
                BuildXzLocalScale(ref ls, scale2D);
                return true;
            }

            if (axes == FrustumFitAxes.YZ)
            {
                BuildYzLocalScale(ref ls, scale2D);
                return true;
            }

            return false;
        }

        private void BuildXzLocalScale(ref Vector3 ls, Vector2 scale2D)
        {
            ls.x = scale2D.x;
            ls.z = scale2D.y;
        }

        private void BuildYzLocalScale(ref Vector3 ls, Vector2 scale2D)
        {
            ls.y = scale2D.x;
            ls.z = scale2D.y;
        }
    }
}
