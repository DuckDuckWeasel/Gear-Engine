using UnityEngine;

namespace Game.GearEngine.Presentation
{
    /// <summary>
    /// Reusable utility for positioning canvas-space UI elements relative to world-space anchors.
    /// Used by any programmatic UI that needs to track a world position projected onto a canvas
    /// (e.g. trash zone, floating labels, tooltips).
    /// </summary>
    public static class CanvasPositionUtility
    {
        /// <summary>
        /// Computes the canvas-local position for a given world position.
        /// </summary>
        /// <param name="canvas">The target canvas.</param>
        /// <param name="worldPos">World-space position to project.</param>
        /// <param name="localPoint">Resulting position in the canvas's local coordinate space.</param>
        /// <returns>True if the conversion succeeded, false if no camera was available.</returns>
        public static bool WorldToCanvasLocal(Canvas canvas, Vector3 worldPos, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (canvas == null)
            {
                return false;
            }

            Camera cam = canvas.worldCamera;
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null)
            {
                return false;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out localPoint);
        }

        /// <summary>
        /// Computes the canvas-local position for a screen-space point (e.g. pointer position).
        /// </summary>
        /// <param name="canvas">The target canvas.</param>
        /// <param name="screenPoint">Screen-space position (e.g. Input.mousePosition).</param>
        /// <param name="localPoint">Resulting position in the canvas's local coordinate space.</param>
        /// <returns>True if the conversion succeeded.</returns>
        public static bool ScreenToCanvasLocal(Canvas canvas, Vector2 screenPoint, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (canvas == null)
            {
                return false;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                cam,
                out localPoint);
        }

        /// <summary>
        /// Positions a RectTransform at a world position projected into canvas space, with pixel offset.
        /// Sets center anchoring (0.5, 0.5) and a configurable pivot for alignment control.
        /// Falls back to a canvas-relative anchor if no camera is available.
        /// </summary>
        /// <param name="rect">The RectTransform to position.</param>
        /// <param name="canvas">The parent canvas.</param>
        /// <param name="worldPos">World-space anchor point.</param>
        /// <param name="offset">Pixel offset in canvas local space applied after projection.</param>
        /// <param name="pivot">
        /// Pivot of the rect relative to the anchor.
        /// (0.5, 0.5) centers the rect on the anchor.
        /// (0, 0.5) aligns the left edge to the anchor.
        /// (1, 0.5) aligns the right edge to the anchor.
        /// </param>
        public static void AnchorToWorldPosition(
            RectTransform rect,
            Canvas canvas,
            Vector3 worldPos,
            Vector2 offset,
            Vector2? pivot = null)
        {
            Vector2 usedPivot = pivot ?? new Vector2(0.5f, 0.5f);

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = usedPivot;

            if (WorldToCanvasLocal(canvas, worldPos, out Vector2 localPoint))
            {
                rect.anchoredPosition = localPoint + offset;
            }
            else
            {
                // Fallback — anchor to top-right of canvas
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-30f, -30f);
            }
        }
    }
}
