using UnityEditor;
using UnityEngine;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;

namespace GearEngine.GearEngine.Editor
{
    [CustomEditor(typeof(FocusPresetSO))]
    public class FocusPresetSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }

                    continue;
                }

                if (iterator.name == "customIndicatorAnchor")
                {
                    SerializedProperty anchorProp = serializedObject.FindProperty("indicatorAnchor");
                    if (anchorProp.enumValueIndex != (int)IndicatorAnchor.Custom)
                    {
                        continue;
                    }
                }

                if (iterator.name == "overlayColor" || iterator.name == "blockClicksOutside")
                {
                    SerializedProperty useDarkProp = serializedObject.FindProperty("useDarkOverlay");
                    if (!useDarkProp.boolValue)
                    {
                        continue;
                    }
                }

                if (iterator.name == "uiEffectPreset")
                {
                    SerializedProperty useUIEffectProp = serializedObject.FindProperty("useUIEffect");
                    if (!useUIEffectProp.boolValue)
                    {
                        continue;
                    }
                }

                EditorGUILayout.PropertyField(iterator, true);
            }

            serializedObject.ApplyModifiedProperties();

            FocusPresetSO preset = (FocusPresetSO)target;

            if (preset.useUIEffect && preset.uiEffectPreset != null)
            {
                EditorGUILayout.Space(15);
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
                EditorGUILayout.Space(10);
                GUILayout.Label("Active Effects", EditorStyles.boldLabel);

                Coffee.UIEffects.UIEffectPreset p = preset.uiEffectPreset;
                string text = "";
                if (p.m_ToneFilter != Coffee.UIEffects.ToneFilter.None)
                {
                    text += $"- Tone: {p.m_ToneFilter}\n";
                }

                if (p.m_ColorFilter != Coffee.UIEffects.ColorFilter.None)
                {
                    text += $"- Color: {p.m_ColorFilter}\n";
                }

                if (p.m_SamplingFilter != Coffee.UIEffects.SamplingFilter.None)
                {
                    text += $"- Filter/Blur: {p.m_SamplingFilter}\n";
                }

                if (p.m_TransitionFilter != Coffee.UIEffects.TransitionFilter.None)
                {
                    text += $"- Transition: {p.m_TransitionFilter}\n";
                }

                if (p.m_ShadowMode != Coffee.UIEffects.ShadowMode.None)
                {
                    text += $"- Shadow: {p.m_ShadowMode}\n";
                }

                if (p.m_GradationMode != Coffee.UIEffects.GradationMode.None)
                {
                    text += $"- Gradation: {p.m_GradationMode}\n";
                }

                if (p.m_EdgeMode != Coffee.UIEffects.EdgeMode.None)
                {
                    text += $"- Edge/Shiny: {p.m_EdgeMode}\n";
                }

                if (p.m_DetailFilter != Coffee.UIEffects.DetailFilter.None)
                {
                    text += $"- Detail: {p.m_DetailFilter}\n";
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = "Preset has no active filters enabled.";
                }

                EditorGUILayout.HelpBox(text.TrimEnd(), MessageType.Info);
            }

            EditorGUILayout.Space(15);
            Rect separator = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(separator, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(10);

            GUILayout.Label("Live Focus Preview", EditorStyles.boldLabel);

            Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            if (Event.current.type == EventType.Repaint)
            {
                DrawFakePreview(previewRect, preset);
            }
        }

        public static void DrawFakePreview(Rect rect, FocusPresetSO preset)
        {
            // 1. Draw dark background
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

            if (preset == null)
            {
                return;
            }

            // 2. Draw Overlay
            if (preset.useDarkOverlay)
            {
                // We draw the overlay color with blending
                Color c = preset.overlayColor;
                Handles.DrawSolidRectangleWithOutline(rect, c, Color.clear);
            }

            // 3. Draw Target
            Vector2 targetSize = new Vector2(100, 50);
            Rect targetRect = new Rect(rect.center.x - targetSize.x / 2, rect.center.y - targetSize.y / 2, targetSize.x, targetSize.y);

            EditorGUI.DrawRect(targetRect, Color.white);
            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            textStyle.normal.textColor = Color.black;
            GUI.Label(targetRect, "Dummy Button", textStyle);

            // 4. Indicator
            Vector2 anchorPos = GetAnchorPosition(targetRect, preset.indicatorAnchor, preset.customIndicatorAnchor);

            Vector2 directionOutward = (anchorPos - targetRect.center).normalized;
            if (directionOutward == Vector2.zero)
            {
                directionOutward = new Vector2(0, -1); // Up in IMGUI
            }

            float previewOffsetScale = GetPreviewOffsetScale(EditorGUIUtility.pixelsPerPoint);
            float previewDirOffset = preset.directionOffset * previewOffsetScale;
            Vector2 previewPosOffset = preset.indicatorOffset * previewOffsetScale;
            previewPosOffset.y = -previewPosOffset.y; // In IMGUI, Y points down, so negate Y for preview to match Unity space

            Vector2 indicatorCenter = anchorPos + previewPosOffset + (directionOutward * previewDirOffset);

            float indSize = 32f;
            Rect indRect = new Rect(indicatorCenter.x - indSize / 2, indicatorCenter.y - indSize / 2, indSize, indSize);

            // Aim to anchor: rotate so indicator points from its position toward target center
            float angle = 0f;
            if (preset.aimToAnchor)
            {
                Vector2 aimDirection = targetRect.center - indicatorCenter; // direction in IMGUI
                Vector2 unityAim = new Vector2(aimDirection.x, -aimDirection.y); // convert to Unity space
                angle = Vector2.SignedAngle(Vector2.up, unityAim);
            }

            Matrix4x4 oldMatrix = GUI.matrix;
            if (preset.aimToAnchor)
            {
                // IMGUI RotateAroundPivot is clockwise. Unity Z rotation is counter-clockwise.
                // We negate the angle to match.
                GUIUtility.RotateAroundPivot(-angle, indicatorCenter);
            }

            Texture2D indTex = null;
            if (preset.indicatorPrefab != null)
            {
                UnityEngine.UI.Image image = preset.indicatorPrefab.GetComponent<UnityEngine.UI.Image>();
                if (image != null && image.sprite != null)
                {
                    indTex = image.sprite.texture;
                }
            }

            if (indTex != null)
            {
                GUI.DrawTexture(indRect, indTex, ScaleMode.ScaleToFit, true);
            }
            else
            {
                EditorGUI.DrawRect(indRect, Color.red);
            }

            GUI.matrix = oldMatrix;
        }

        public static float GetPreviewOffsetScale(float pixelsPerPoint)
        {
            return TutorialFocusService.k_offsetPixelsPerUnit / Mathf.Max(1f, pixelsPerPoint);
        }

        public static Vector2 GetAnchorPosition(Rect targetRect, IndicatorAnchor anchor, Vector2 customAnchor)
        {
            switch (anchor)
            {
                case IndicatorAnchor.TopLeft: return new Vector2(targetRect.xMin, targetRect.yMin); // Note: IMGUI Y is down
                case IndicatorAnchor.TopCenter: return new Vector2(targetRect.center.x, targetRect.yMin);
                case IndicatorAnchor.TopRight: return new Vector2(targetRect.xMax, targetRect.yMin);
                case IndicatorAnchor.MiddleRight: return new Vector2(targetRect.xMax, targetRect.center.y);
                case IndicatorAnchor.BottomRight: return new Vector2(targetRect.xMax, targetRect.yMax);
                case IndicatorAnchor.BottomCenter: return new Vector2(targetRect.center.x, targetRect.yMax);
                case IndicatorAnchor.BottomLeft: return new Vector2(targetRect.xMin, targetRect.yMax);
                case IndicatorAnchor.MiddleLeft: return new Vector2(targetRect.xMin, targetRect.center.y);
                case IndicatorAnchor.MiddleCenter: return targetRect.center;
                case IndicatorAnchor.Custom:
                    return new Vector2(
                        Mathf.Lerp(targetRect.xMin, targetRect.xMax, customAnchor.x),
                        Mathf.Lerp(targetRect.yMax, targetRect.yMin, customAnchor.y) // Y inverted for IMGUI
                    );
                default: return targetRect.center;
            }
        }
    }
}
