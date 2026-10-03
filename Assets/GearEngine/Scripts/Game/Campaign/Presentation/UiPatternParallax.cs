using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class UiPatternParallax : MonoBehaviour
    {
        private const string k_baseLayerName = "PatternBase";
        private const string k_scrollLayerName = "PatternScroll";
        private const float k_horizontalTileCount = 1.5f;
        private static readonly Vector2 s_scrollVelocity = new Vector2(0.018f, 0.026f);

        [SerializeField] private Texture2D backgroundTexture;
        [SerializeField] private Texture2D patternTexture;
        private Image sourceImage;
        private RawImage baseLayer;
        private RawImage scrollingLayer;
        private RectTransform rectTransform;
        private Vector2 scrollOffset;

        private void OnEnable()
        {
            Initialize();
        }

        private void Update()
        {
            Advance(Time.unscaledDeltaTime);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (scrollingLayer != null)
            {
                ApplyUvRect();
            }
        }

        private void Initialize()
        {
            if (backgroundTexture == null || patternTexture == null)
            {
                Debug.LogError("[UiPatternParallax] Background and repeatable pattern textures are required.");
                return;
            }

            sourceImage ??= GetComponent<Image>();
            rectTransform ??= (RectTransform)transform;
            sourceImage.enabled = false;

            baseLayer = GetOrCreateLayer(k_baseLayerName, 0);
            baseLayer.texture = backgroundTexture;
            baseLayer.color = Color.white;
            baseLayer.uvRect = new Rect(0f, 0f, 1f, 1f);

            scrollingLayer = GetOrCreateLayer(k_scrollLayerName, 1);
            scrollingLayer.texture = patternTexture;
            scrollingLayer.color = Color.white;
            ApplyUvRect();
        }

        private void Advance(float deltaTime)
        {
            if (scrollingLayer == null)
            {
                return;
            }

            scrollOffset.x = Mathf.Repeat(scrollOffset.x + (s_scrollVelocity.x * deltaTime), 1f);
            scrollOffset.y = Mathf.Repeat(scrollOffset.y + (s_scrollVelocity.y * deltaTime), 1f);
            ApplyUvRect();
        }

        private void ApplyUvRect()
        {
            float width = Mathf.Max(rectTransform.rect.width, 1f);
            float height = Mathf.Max(rectTransform.rect.height, 1f);
            float textureAspect = patternTexture.width / (float)patternTexture.height;
            float rectAspect = width / height;
            float verticalTileCount = Mathf.Max(
                k_horizontalTileCount,
                k_horizontalTileCount * textureAspect / rectAspect);
            scrollingLayer.uvRect = new Rect(
                scrollOffset.x,
                scrollOffset.y,
                k_horizontalTileCount,
                verticalTileCount);
        }

        private RawImage GetOrCreateLayer(string layerName, int siblingIndex)
        {
            Transform existing = transform.Find(layerName);
            GameObject layerObject = existing != null
                ? existing.gameObject
                : new GameObject(layerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            if (existing == null)
            {
                layerObject.transform.SetParent(transform, false);
            }

            RectTransform layerTransform = (RectTransform)layerObject.transform;
            layerTransform.anchorMin = Vector2.zero;
            layerTransform.anchorMax = Vector2.one;
            layerTransform.offsetMin = Vector2.zero;
            layerTransform.offsetMax = Vector2.zero;
            layerTransform.localRotation = Quaternion.identity;
            layerTransform.localScale = Vector3.one;
            layerTransform.SetSiblingIndex(siblingIndex);

            RawImage layer = layerObject.GetComponent<RawImage>();
            layer.raycastTarget = false;
            return layer;
        }

        public static UiPatternParallax Attach(Transform viewRoot)
        {
            UiPatternParallax scroll = viewRoot != null ? viewRoot.GetComponentInChildren<UiPatternParallax>(true) : null;
            if (scroll == null)
            {
                Debug.LogError("[UiPatternParallax] The background prefab requires a configured scrolling component.");
                return null;
            }

            scroll.Initialize();
            return scroll;
        }
    }
}
