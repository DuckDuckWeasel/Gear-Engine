using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.World
{
    public readonly struct FrustumBounds
    {
        public FrustumBounds(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public readonly float Width;
        public readonly float Height;

        public static FrustumBounds FromCamera(bool isOrthographic, float orthographicSize, float fieldOfViewDegrees, float aspect, float depth)
        {
            if (aspect <= 0f)
            {
                throw new ArgumentException($"aspect must be positive, got {aspect}.", nameof(aspect));
            }

            float height = BuildFrustumHeight(isOrthographic, orthographicSize, fieldOfViewDegrees, depth);
            return new FrustumBounds(height * aspect, height);
        }

        private static float BuildFrustumHeight(bool isOrthographic, float orthographicSize, float fieldOfViewDegrees, float depth)
        {
            if (isOrthographic)
            {
                return orthographicSize * 2f;
            }

            if (depth <= 0f)
            {
                throw new ArgumentException($"depth must be positive for perspective cameras, got {depth}.", nameof(depth));
            }

            return 2f * Mathf.Tan(fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad) * depth;
        }
    }
}
