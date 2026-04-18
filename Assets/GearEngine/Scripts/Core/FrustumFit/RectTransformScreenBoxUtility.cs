using System;
using UnityEngine;

namespace GearEngine.FrustumFit
{
    /// <summary>
    /// Converts a laid-out <see cref="RectTransform"/> screen box into viewport min/max for a given <see cref="Camera"/>.
    /// </summary>
    public static class RectTransformScreenBoxUtility
    {
        private static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>
        /// Resolves the canvas used for UI → screen projection. When <paramref name="canvas"/> is null, uses <paramref name="sourceRect"/>'s parent canvas.
        /// </summary>
        public static Canvas ResolveCanvas(RectTransform sourceRect, Canvas canvas)
        {
            if (sourceRect == null)
            {
                throw new ArgumentNullException(nameof(sourceRect));
            }

            return canvas != null ? canvas : sourceRect.GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// Projects <paramref name="sourceRect"/> corners to <paramref name="worldCamera"/> viewport space.
        /// Returns axis-aligned viewport bounds (min corner, max corner) in normalized viewport coordinates.
        /// </summary>
        public static void GetViewportBounds(RectTransform sourceRect, Canvas canvas, Camera worldCamera, out Vector2 viewportMin, out Vector2 viewportMax)
        {
            if (worldCamera == null)
            {
                throw new ArgumentNullException(nameof(worldCamera));
            }

            Canvas resolved = ResolveCanvas(sourceRect, canvas);
            if (resolved == null)
            {
                throw new InvalidOperationException("RectTransform has no Canvas in parents and no canvas was provided.");
            }

            Camera uiCamera = resolved.renderMode == RenderMode.ScreenSpaceOverlay ? null : resolved.worldCamera;

            sourceRect.GetWorldCorners(Corners);
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, Corners[i]);
                Vector3 vp = worldCamera.ScreenToViewportPoint(screen);
                minX = Mathf.Min(minX, vp.x);
                minY = Mathf.Min(minY, vp.y);
                maxX = Mathf.Max(maxX, vp.x);
                maxY = Mathf.Max(maxY, vp.y);
            }

            viewportMin = new Vector2(minX, minY);
            viewportMax = new Vector2(maxX, maxY);
        }

        /// <summary>
        /// Viewport size (max - min) and center ((min + max) * 0.5).
        /// </summary>
        public static void GetViewportSizeAndCenter(Vector2 viewportMin, Vector2 viewportMax, out Vector2 viewportSize, out Vector2 viewportCenter)
        {
            viewportSize = viewportMax - viewportMin;
            viewportCenter = (viewportMin + viewportMax) * 0.5f;
        }
    }
}
