using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public static class BoardScreenPositionUtility
    {
        public static bool TryGetLocalPoint(RectTransform boardRect, Canvas canvas, Vector2 screenPosition, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            if (boardRect == null || canvas == null)
            {
                return false;
            }

            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect, screenPosition, eventCamera, out localPoint);
        }
    }
}
