using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.World
{
    /// <summary>
    /// Pure-math helpers for the FrustumFit system. No MonoBehaviour, no Unity lifecycle.
    /// All methods are stateless and safe to call from EditMode tests.
    /// </summary>
    public static class FrustumFitMath
    {
        /// <summary>
        /// Computes the world-space frustum bounds at a given depth.
        /// Thin wrapper over <see cref="FrustumBounds.FromCamera"/> kept here so callers
        /// only need to import one type.
        /// </summary>
        public static FrustumBounds ComputeBounds(
            bool  isOrthographic,
            float orthographicSize,
            float fieldOfViewDegrees,
            float aspect,
            float depth)
        {
            return FrustumBounds.FromCamera(isOrthographic, orthographicSize, fieldOfViewDegrees, aspect, depth);
        }

        /// <summary>
        /// Returns the (localScaleX, localScaleY) pair to apply to the two screen-facing
        /// axes of an object so it occupies the configured fraction of the frustum.
        /// </summary>
        /// <param name="bounds">Frustum world-space dimensions at the object's depth.</param>
        /// <param name="fillX">Fraction of frustum width to fill (0–1 typical; &gt;1 for intentional overflow).</param>
        /// <param name="fillY">Fraction of frustum height to fill.</param>
        /// <param name="mode">How the object fills the target box.</param>
        /// <param name="meshSize">Natural size of the mesh in local space on the two screen-facing axes (X = horizontal, Y = vertical).</param>
        /// <param name="parentLossyScale">World scale of the parent transform on the two screen-facing axes. Pass Vector2.one if there is no parent.</param>
        /// <returns>The localScale components to write to the two screen-facing axes.</returns>
        public static Vector2 ComputeLocalScale(
            FrustumBounds  bounds,
            float          fillX,
            float          fillY,
            FrustumFillMode mode,
            Vector2        meshSize,
            Vector2        parentLossyScale)
        {
            if (meshSize.x <= 0f)
                throw new ArgumentException($"meshSize.x must be positive, got {meshSize.x}.", nameof(meshSize));
            if (meshSize.y <= 0f)
                throw new ArgumentException($"meshSize.y must be positive, got {meshSize.y}.", nameof(meshSize));
            if (Mathf.Approximately(parentLossyScale.x, 0f))
                throw new ArgumentException($"parentLossyScale.x must not be zero.", nameof(parentLossyScale));
            if (Mathf.Approximately(parentLossyScale.y, 0f))
                throw new ArgumentException($"parentLossyScale.y must not be zero.", nameof(parentLossyScale));

            float targetWidth  = bounds.Width  * fillX;
            float targetHeight = bounds.Height * fillY;

            float rawScaleX = targetWidth  / meshSize.x;
            float rawScaleY = targetHeight / meshSize.y;

            float worldScaleX, worldScaleY;

            switch (mode)
            {
                case FrustumFillMode.Stretch:
                    worldScaleX = rawScaleX;
                    worldScaleY = rawScaleY;
                    break;

                case FrustumFillMode.Fit:
                    float fitUniform = Mathf.Min(rawScaleX, rawScaleY);
                    worldScaleX = fitUniform;
                    worldScaleY = fitUniform;
                    break;

                case FrustumFillMode.Fill:
                    float fillUniform = Mathf.Max(rawScaleX, rawScaleY);
                    worldScaleX = fillUniform;
                    worldScaleY = fillUniform;
                    break;

                case FrustumFillMode.FillWidth:
                    worldScaleX = rawScaleX;
                    worldScaleY = rawScaleX;
                    break;

                case FrustumFillMode.FillHeight:
                    worldScaleX = rawScaleY;
                    worldScaleY = rawScaleY;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unhandled FrustumFillMode value.");
            }

            return new Vector2(
                worldScaleX / parentLossyScale.x,
                worldScaleY / parentLossyScale.y);
        }
    }
}
