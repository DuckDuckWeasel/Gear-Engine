using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaRectTransform : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private RectTransform target;

        private void OnEnable()
        {
            target = GetComponent<RectTransform>();
            ApplyIfChanged(force: true);
        }

        private void LateUpdate()
        {
            ApplyIfChanged(force: false);
        }

        private void ApplyIfChanged(bool force)
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (!force && safeArea == lastSafeArea && screenSize == lastScreenSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreenSize = screenSize;
            if (target == null || screenSize.x <= 0 || screenSize.y <= 0)
            {
                return;
            }

            ApplyAnchors(safeArea, screenSize);
        }

        private void ApplyAnchors(Rect safeArea, Vector2Int screenSize)
        {
            ToAnchors(safeArea, screenSize, out Vector2 anchorMin, out Vector2 anchorMax);
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        internal static void ToAnchors(Rect safeArea, Vector2Int screenSize, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            anchorMin = safeArea.position;
            anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;
        }
    }
}
