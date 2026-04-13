using System;
using UnityEngine;

namespace Scaffold.GearEngine.Presentation.World
{
    /// <summary>
    /// World-space width and height of the camera frustum at a given depth.
    /// For orthographic cameras the depth parameter has no effect on the result.
    /// </summary>
    public readonly struct FrustumBounds
    {
        public readonly float Width;
        public readonly float Height;

        public FrustumBounds(float width, float height)
        {
            Width  = width;
            Height = height;
        }

        /// <summary>
        /// Computes frustum bounds from raw camera parameters so this can be
        /// called without a live Camera reference (useful in tests).
        /// </summary>
        /// <param name="isOrthographic">True if the camera uses orthographic projection.</param>
        /// <param name="orthographicSize">Camera.orthographicSize (half the vertical world-unit span).</param>
        /// <param name="fieldOfViewDegrees">Camera.fieldOfView in degrees (perspective only).</param>
        /// <param name="aspect">Camera.aspect (width / height).</param>
        /// <param name="depth">World-space distance from the camera at which to sample the frustum (perspective only).</param>
        public static FrustumBounds FromCamera(
            bool  isOrthographic,
            float orthographicSize,
            float fieldOfViewDegrees,
            float aspect,
            float depth)
        {
            if (aspect <= 0f)
                throw new ArgumentException($"aspect must be positive, got {aspect}.", nameof(aspect));

            float height;
            if (isOrthographic)
            {
                height = orthographicSize * 2f;
            }
            else
            {
                if (depth <= 0f)
                    throw new ArgumentException($"depth must be positive for perspective cameras, got {depth}.", nameof(depth));

                height = 2f * Mathf.Tan(fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad) * depth;
            }

            return new FrustumBounds(height * aspect, height);
        }
    }
}
