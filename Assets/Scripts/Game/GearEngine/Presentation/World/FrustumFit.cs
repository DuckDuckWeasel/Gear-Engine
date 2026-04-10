using System;
using UnityEngine;

namespace Game.GearEngine.Presentation.World
{
    /// <summary>
    /// Scales a world-space GameObject so it occupies a configured fraction of
    /// the camera frustum at a given depth. Drop onto any GameObject that has a
    /// Renderer (or whose child has one — see _renderer field below).
    ///
    /// Call Apply() externally (e.g. on screen resize) or enable _applyEveryFrame
    /// for continuous updates. Apply() is idempotent: calling it multiple times
    /// with no state change produces the same localScale output.
    /// </summary>
    public sealed class FrustumFit : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Camera used to measure the frustum. Defaults to Camera.main if left empty.")]
        [SerializeField] private Camera _camera;

        [Tooltip("World-space distance from the camera at which frustum dimensions are sampled. " +
                 "Set this to the object's Z distance from the camera. " +
                 "Has no effect on orthographic cameras.")]
        [SerializeField] private float _depth = 10f;

        [Header("Fill")]
        [Tooltip("Fraction of the frustum width the object should occupy. " +
                 "1 = full width, 0.5 = half width. Values > 1 intentionally overflow the screen.")]
        [SerializeField, Range(0f, 2f)] private float _fillX = 1f;

        [Tooltip("Fraction of the frustum height the object should occupy.")]
        [SerializeField, Range(0f, 2f)] private float _fillY = 1f;

        [Tooltip("How the object fills the configured target box.")]
        [SerializeField] private FrustumFillMode _mode = FrustumFillMode.Fit;

        [Header("Axes")]
        [Tooltip("Which two local axes map to screen horizontal and vertical. " +
                 "XY = Quad/sprite (default). XZ = Unity Plane rotated to face camera. YZ = rare custom meshes.")]
        [SerializeField] private FrustumFitAxes _axes = FrustumFitAxes.XY;

        [Header("Update Timing")]
        [Tooltip("Call Apply() automatically on Start.")]
        [SerializeField] private bool _applyOnStart = true;

        [Tooltip("Call Apply() every LateUpdate. Use only when the camera, window size, " +
                 "or fill settings change at runtime. Disable and call Apply() externally " +
                 "from a resize coordinator for better performance.")]
        [SerializeField] private bool _applyEveryFrame = false;

        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();

            if (_renderer == null)
                Debug.LogError($"[FrustumFit] No Renderer found on '{name}'. " +
                               "Attach FrustumFit to the GameObject that owns the Renderer, " +
                               "or move the Renderer to this GameObject.");

            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                Debug.LogError($"[FrustumFit] No camera assigned and Camera.main is null on '{name}'. " +
                               "Assign a camera in the Inspector.");
        }

        private void Start()
        {
            if (_applyOnStart)
                Apply();
        }

        private void LateUpdate()
        {
            if (_applyEveryFrame)
                Apply();
        }

        /// <summary>
        /// Computes and applies the localScale required to fill the configured fraction
        /// of the camera frustum. Safe to call at any time; does nothing if required
        /// references are missing and logs an error instead.
        /// </summary>
        public void Apply()
        {
            try
            {
                if (_camera == null)
                {
                    Debug.LogError($"[FrustumFit] Cannot Apply — camera is null on '{name}'.");
                    return;
                }

                if (_renderer == null)
                {
                    Debug.LogError($"[FrustumFit] Cannot Apply — Renderer is null on '{name}'.");
                    return;
                }

                FrustumBounds bounds = FrustumFitMath.ComputeBounds(
                    _camera.orthographic,
                    _camera.orthographicSize,
                    _camera.fieldOfView,
                    _camera.aspect,
                    _depth);

                Vector3 boundsSize  = _renderer.localBounds.size;
                Vector2 meshSize    = ExtractAxesPair(boundsSize, _axes);
                Vector2 parentScale = GetParentLossyScalePair(_axes);

                Vector2 localScale2D = FrustumFitMath.ComputeLocalScale(
                    bounds, _fillX, _fillY, _mode, meshSize, parentScale);

                ApplyLocalScalePair(localScale2D, _axes);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FrustumFit] Apply failed on '{name}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        // --- Private helpers -------------------------------------------------

        private static Vector2 ExtractAxesPair(Vector3 v, FrustumFitAxes axes)
        {
            return axes switch
            {
                FrustumFitAxes.XY => new Vector2(v.x, v.y),
                FrustumFitAxes.XZ => new Vector2(v.x, v.z),
                FrustumFitAxes.YZ => new Vector2(v.y, v.z),
                _                 => throw new ArgumentOutOfRangeException(nameof(axes), axes, null),
            };
        }

        private Vector2 GetParentLossyScalePair(FrustumFitAxes axes)
        {
            Vector3 ps = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            return ExtractAxesPair(ps, axes);
        }

        private void ApplyLocalScalePair(Vector2 scale2D, FrustumFitAxes axes)
        {
            Vector3 ls = transform.localScale;

            switch (axes)
            {
                case FrustumFitAxes.XY:
                    ls.x = scale2D.x;
                    ls.y = scale2D.y;
                    break;
                case FrustumFitAxes.XZ:
                    ls.x = scale2D.x;
                    ls.z = scale2D.y;
                    break;
                case FrustumFitAxes.YZ:
                    ls.y = scale2D.x;
                    ls.z = scale2D.y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axes), axes, null);
            }

            transform.localScale = ls;
        }
    }
}
