using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Coffee.UIEffects;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;

namespace GearEngine.GearEngine.Editor
{
    public class TutorialFocusPreviewWindow : EditorWindow
    {
        private FocusPresetSO currentPreset;
        private PreviewRenderUtility previewUtility;

        // Dummy Settings
        private IndicatorAnchor simulatedAnchor = IndicatorAnchor.MiddleCenter;
        private float simulatedDirectionOffset = 0f;
        private bool simulatedAimToAnchor = false;

        // Internal GameObjects
        private GameObject canvasGo;
        private GameObject targetGo;
        private Image targetImage;
        private GameObject indicatorGo;
        private UIEffect targetEffect;
        private GameObject overlayGo;
        private Image overlayImage;

        public void SetPreset(FocusPresetSO preset)
        {
            currentPreset = preset;
            RefreshPreview();
        }

        public void SetPreviewParams(IndicatorAnchor anchor, float directionOffset, bool aimToAnchor)
        {
            simulatedAnchor = anchor;
            simulatedDirectionOffset = directionOffset;
            simulatedAimToAnchor = aimToAnchor;
            RefreshPreview();
        }

        private void OnEnable()
        {
            if (previewUtility == null)
            {
                previewUtility = new PreviewRenderUtility();
                previewUtility.camera.orthographic = true;
                previewUtility.camera.orthographicSize = 5f;
                previewUtility.camera.transform.position = new Vector3(0, 0, -10f);
                previewUtility.camera.transform.rotation = Quaternion.identity;
                previewUtility.camera.farClipPlane = 20f;
            }

            SetupPreviewScene();
        }

        private void OnDisable()
        {
            if (previewUtility != null)
            {
                previewUtility.Cleanup();
                previewUtility = null;
            }
        }

        private void SetupPreviewScene()
        {
            if (previewUtility == null) return;

            // Clean previous
            if (canvasGo != null) DestroyImmediate(canvasGo);

            // Canvas
            canvasGo = new GameObject("PreviewCanvas", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10); // World units
            previewUtility.AddSingleGO(canvasGo);

            // Overlay
            overlayGo = new GameObject("Overlay", typeof(Image));
            overlayGo.transform.SetParent(canvasGo.transform, false);
            overlayImage = overlayGo.GetComponent<Image>();
            RectTransform overlayRect = overlayGo.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Target
            targetGo = new GameObject("DummyTarget", typeof(Image));
            targetGo.transform.SetParent(canvasGo.transform, false);
            targetImage = targetGo.GetComponent<Image>();
            targetImage.color = Color.white;
            RectTransform targetRect = targetGo.GetComponent<RectTransform>();
            targetRect.sizeDelta = new Vector2(4, 2);

            // Text inside target
            GameObject textGo = new GameObject("Text", typeof(Text));
            textGo.transform.SetParent(targetGo.transform, false);
            Text text = textGo.GetComponent<Text>();
            text.text = "Dummy Button";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = 1;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Indicator
            indicatorGo = new GameObject("DummyIndicator", typeof(Image));
            indicatorGo.transform.SetParent(canvasGo.transform, false);
            Image indImg = indicatorGo.GetComponent<Image>();
            indImg.color = Color.red; // Default generic arrow look
            indicatorGo.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);

            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (currentPreset == null || targetGo == null) return;

            // Overlay
            if (currentPreset.useDarkOverlay)
            {
                overlayImage.enabled = true;
                overlayImage.color = currentPreset.overlayColor;
            }
            else
            {
                overlayImage.enabled = false;
            }

            // UIEffect
            if (targetEffect != null)
            {
                DestroyImmediate(targetEffect);
            }

            if (currentPreset.useUIEffect && currentPreset.uiEffectPreset != null)
            {
                targetEffect = targetGo.AddComponent<UIEffect>();
                targetEffect.LoadPreset(currentPreset.uiEffectPreset, false);
            }

            // Indicator Logic
            RectTransform targetRect = targetGo.GetComponent<RectTransform>();
            RectTransform indRect = indicatorGo.GetComponent<RectTransform>();

            Vector2 anchorPos = GetAnchorPosition(targetRect, simulatedAnchor);
            
            // Direction outward
            Vector2 targetCenter = targetRect.rect.center;
            Vector2 directionOutward = (anchorPos - targetCenter).normalized;
            if (directionOutward == Vector2.zero) directionOutward = Vector2.up;

            indRect.anchoredPosition = anchorPos + (directionOutward * simulatedDirectionOffset);
            indRect.rotation = Quaternion.identity;

            if (simulatedAimToAnchor)
            {
                float angle = Mathf.Atan2(-directionOutward.y, -directionOutward.x) * Mathf.Rad2Deg;
                indRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        }

        private Vector2 GetAnchorPosition(RectTransform targetRect, IndicatorAnchor anchor)
        {
            Rect rect = targetRect.rect;
            switch (anchor)
            {
                case IndicatorAnchor.TopLeft: return new Vector2(rect.xMin, rect.yMax);
                case IndicatorAnchor.TopCenter: return new Vector2(rect.center.x, rect.yMax);
                case IndicatorAnchor.TopRight: return new Vector2(rect.xMax, rect.yMax);
                case IndicatorAnchor.MiddleRight: return new Vector2(rect.xMax, rect.center.y);
                case IndicatorAnchor.BottomRight: return new Vector2(rect.xMax, rect.yMin);
                case IndicatorAnchor.BottomCenter: return new Vector2(rect.center.x, rect.yMin);
                case IndicatorAnchor.BottomLeft: return new Vector2(rect.xMin, rect.yMin);
                case IndicatorAnchor.MiddleLeft: return new Vector2(rect.xMin, rect.center.y);
                case IndicatorAnchor.MiddleCenter: return rect.center;
                case IndicatorAnchor.Custom: return rect.center;
                default: return rect.center;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Live Tutorial Preview", EditorStyles.boldLabel);

            currentPreset = (FocusPresetSO)EditorGUILayout.ObjectField("Preset", currentPreset, typeof(FocusPresetSO), false);

            if (currentPreset == null)
            {
                EditorGUILayout.HelpBox("Select a FocusPresetSO to preview.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Simulate Settings (Fungus Simulator)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            simulatedAnchor = (IndicatorAnchor)EditorGUILayout.EnumPopup("Indicator Anchor", simulatedAnchor);
            simulatedDirectionOffset = EditorGUILayout.Slider("Direction Offset", simulatedDirectionOffset, -5f, 5f);
            simulatedAimToAnchor = EditorGUILayout.Toggle("Aim To Anchor", simulatedAimToAnchor);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshPreview();
            }

            if (GUILayout.Button("Force Refresh"))
            {
                RefreshPreview();
            }

            EditorGUILayout.Space();

            // Draw Preview
            Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (previewUtility != null)
            {
                previewUtility.BeginPreview(previewRect, GUIStyle.none);
                
                // Force Canvas update
                Canvas.ForceUpdateCanvases();

                previewUtility.camera.Render();
                Texture previewTex = previewUtility.EndPreview();
                GUI.DrawTexture(previewRect, previewTex, ScaleMode.StretchToFill, false);
            }
        }
    }
}
