using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public static class CanvasPositionUtility
    {
        public static bool WorldToCanvasLocal(Canvas canvas, Vector3 worldPos, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (canvas == null)
            {
                return false;
            }

            Camera cam = ResolveWorldCamera(canvas);
            if (cam == null)
            {
                return false;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            Camera overlayCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, overlayCam, out localPoint);
        }

        public static bool ScreenToCanvasLocal(Canvas canvas, Vector2 screenPoint, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (canvas == null)
            {
                return false;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint);
        }

        public static void AnchorToWorldPosition(RectTransform rect, Canvas canvas, Vector3 worldPos, Vector2 offset, Vector2? pivot = null)
        {
            Vector2 usedPivot = pivot ?? new Vector2(0.5f, 0.5f);
            ApplyCenterAnchors(rect, usedPivot);

            if (WorldToCanvasLocal(canvas, worldPos, out Vector2 localPoint))
            {
                rect.anchoredPosition = localPoint + offset;
            }
            else
            {
                ApplyCanvasFallbackAnchor(rect);
            }
        }

        private static Camera ResolveWorldCamera(Canvas canvas)
        {
            Camera cam = canvas.worldCamera;
            if (cam == null)
            {
                cam = Camera.main;
            }

            return cam;
        }

        private static void ApplyCenterAnchors(RectTransform rect, Vector2 usedPivot)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = usedPivot;
        }

        private static void ApplyCanvasFallbackAnchor(RectTransform rect)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-30f, -30f);
        }
    }
}
