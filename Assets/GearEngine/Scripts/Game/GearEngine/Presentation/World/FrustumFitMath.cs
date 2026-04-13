using System;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.World
{
    public static class FrustumFitMath
    {
        public static FrustumBounds ComputeBounds(bool isOrthographic, float orthographicSize, float fieldOfViewDegrees, float aspect, float depth)
        {
            return FrustumBounds.FromCamera(isOrthographic, orthographicSize, fieldOfViewDegrees, aspect, depth);
        }

        public static Vector2 ComputeLocalScale(FrustumBounds bounds, float fillX, float fillY, FrustumFillMode mode, Vector2 meshSize, Vector2 parentLossyScale)
        {
            ThrowIfInvalidMeshSize(meshSize);
            ThrowIfInvalidParentScale(parentLossyScale);
            float targetWidth = bounds.Width * fillX;
            float targetHeight = bounds.Height * fillY;
            float rawScaleX = targetWidth / meshSize.x;
            float rawScaleY = targetHeight / meshSize.y;
            Vector2 worldScale = ComputeWorldScalePair(mode, rawScaleX, rawScaleY);
            return new Vector2(worldScale.x / parentLossyScale.x, worldScale.y / parentLossyScale.y);
        }

        private static void ThrowIfInvalidMeshSize(Vector2 meshSize)
        {
            if (meshSize.x <= 0f)
            {
                throw new ArgumentException($"meshSize.x must be positive, got {meshSize.x}.", nameof(meshSize));
            }

            if (meshSize.y <= 0f)
            {
                throw new ArgumentException($"meshSize.y must be positive, got {meshSize.y}.", nameof(meshSize));
            }
        }

        private static void ThrowIfInvalidParentScale(Vector2 parentLossyScale)
        {
            if (Mathf.Approximately(parentLossyScale.x, 0f))
            {
                throw new ArgumentException("parentLossyScale.x must not be zero.", nameof(parentLossyScale));
            }

            if (Mathf.Approximately(parentLossyScale.y, 0f))
            {
                throw new ArgumentException("parentLossyScale.y must not be zero.", nameof(parentLossyScale));
            }
        }

        private static Vector2 ComputeWorldScalePair(FrustumFillMode mode, float rawScaleX, float rawScaleY)
        {
            return mode switch
            {
                FrustumFillMode.Stretch => new Vector2(rawScaleX, rawScaleY),
                FrustumFillMode.Fit => CreateUniformScale(Mathf.Min(rawScaleX, rawScaleY)),
                FrustumFillMode.Fill => CreateUniformScale(Mathf.Max(rawScaleX, rawScaleY)),
                FrustumFillMode.FillWidth => new Vector2(rawScaleX, rawScaleX),
                FrustumFillMode.FillHeight => new Vector2(rawScaleY, rawScaleY),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unhandled FrustumFillMode value."),
            };
        }

        private static Vector2 CreateUniformScale(float uniform)
        {
            return new Vector2(uniform, uniform);
        }
    }
}
