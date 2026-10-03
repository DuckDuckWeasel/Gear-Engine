using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>Scales the authored 390-unit HUD and reserves a separate road rectangle.</summary>
    public sealed class ClubSportHudLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform header;
        [SerializeField] private RectTransform feedback;
        [SerializeField] private RectTransform dashboard;
        [SerializeField] private RectTransform boardArea;
        [SerializeField] private RectTransform roadArea;
        [SerializeField] private RectTransform pulseButton;
        [SerializeField] private RectTransform turboButton;
        private Vector2 previousSize;
        private Rect previousSafeArea;

        internal bool Refresh(bool force = false)
        {
            RectTransform root = (RectTransform)transform;
            RectTransform parent = root.parent as RectTransform;
            if (parent == null || parent.rect.width < 1f)
            {
                return false;
            }
            Rect safe = Screen.safeArea;
            if (!force && previousSize == parent.rect.size && safe == previousSafeArea)
            {
                return false;
            }
            previousSize = parent.rect.size;
            previousSafeArea = safe;
            Vector2 minimum = new Vector2(safe.xMin / Mathf.Max(1f, Screen.width), safe.yMin / Mathf.Max(1f, Screen.height));
            Vector2 maximum = new Vector2(safe.xMax / Mathf.Max(1f, Screen.width), safe.yMax / Mathf.Max(1f, Screen.height));
            root.anchorMin = minimum;
            root.anchorMax = maximum;
            root.offsetMin = root.offsetMax = Vector2.zero;
            Canvas canvas = GetComponentInParent<Canvas>();
            float pixels = canvas != null ? canvas.pixelRect.width : Screen.width;
            ApplySize(root.rect.size, Mathf.Min(390f, pixels * (maximum.x - minimum.x)));
            return true;
        }

        internal void ApplySize(Vector2 available, float referenceWidth = 390f)
        {
            float width = Mathf.Min(referenceWidth, available.x);
            float scale = available.x / Mathf.Max(1f, width);
            // A short intermediate window (including orientation changes) must not collapse
            // the road to one pixel before world-sized props are generated.
            scale = Mathf.Min(scale, Mathf.Max(1f, available.y) / 560f);
            width = available.x / Mathf.Max(.001f, scale);
            float height = available.y / Mathf.Max(.001f, scale);
            content.sizeDelta = new Vector2(width, height);
            content.localScale = Vector3.one * scale;
            float headerHeight = height * .09f;
            float boardHeight = Mathf.Clamp(height * .22f, 140f, 172f);
            float dashboardHeight = Mathf.Clamp(height * .13f, 84f, 102f);
            Place(header, new Rect(0f, height - headerHeight, width, headerHeight));
            Place(feedback, new Rect(0f, height - headerHeight - 46f, width, 46f));
            Place(boardArea, new Rect(0f, 0f, width, boardHeight));
            Place(dashboard, new Rect(0f, boardHeight, width, dashboardHeight));
            float roadBottom = boardHeight + dashboardHeight + 16f;
            float roadTop = height - headerHeight - 46f - 16f;
            Place(roadArea, new Rect(width * .08f, roadBottom, width * .84f, Mathf.Max(1f, roadTop - roadBottom)));
            float buttonWidth = width < 375f ? 62f : 80f;
            float margin = width < 375f ? 8f : 14f;
            pulseButton.GetComponentInChildren<TMPro.TMP_Text>().fontSize = width < 375f ? 8f : 9f;
            turboButton.GetComponentInChildren<TMPro.TMP_Text>().fontSize = width < 375f ? 8f : 9f;
            pulseButton.sizeDelta = turboButton.sizeDelta = new Vector2(buttonWidth, 44f);
            pulseButton.anchoredPosition = new Vector2(margin + buttonWidth * .5f, 0f);
            turboButton.anchoredPosition = new Vector2(-margin - buttonWidth * .5f, 0f);
        }

        internal Rect RoadAnchors() => NormalizedArea(roadArea);
        internal Rect BoardAnchors() => NormalizedArea(boardArea);

        private Rect NormalizedArea(RectTransform area)
        {
            // All authored areas are direct children of Content. Local layout stays valid while the
            // screen's opening animation has scale zero and across canvases with different render modes.
            RectTransform root = (RectTransform)transform;
            Vector2 minimum = area.anchoredPosition / content.sizeDelta;
            Vector2 maximum = (area.anchoredPosition + area.sizeDelta) / content.sizeDelta;
            Vector2 safeSize = root.anchorMax - root.anchorMin;
            minimum = root.anchorMin + minimum * safeSize;
            maximum = root.anchorMin + maximum * safeSize;
            return Rect.MinMaxRect(minimum.x, minimum.y, maximum.x, maximum.y);
        }

        private static void Place(RectTransform rect, Rect bounds)
        {
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = bounds.position;
            rect.sizeDelta = bounds.size;
        }
    }
}
