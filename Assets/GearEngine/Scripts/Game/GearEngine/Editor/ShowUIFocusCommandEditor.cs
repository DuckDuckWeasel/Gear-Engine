using Fungus.EditorUtils;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;
using UnityEditor;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    [CustomEditor(typeof(ShowUIFocusCommand))]
    public class ShowUIFocusCommandEditor : CommandEditor
    {
        public override void DrawCommandGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.DrawCommandGUI();
            if (EditorGUI.EndChangeCheck())
            {
                // Nothing to refresh, it draws directly in Repaint
            }

            var presetProp = serializedObject.FindProperty("_preset");
            if (presetProp.objectReferenceValue != null)
            {
                EditorGUILayout.Space(15);
                Rect sepRect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 1));
                EditorGUILayout.Space(10);
                
                GUILayout.Label("Live Focus Preview", EditorStyles.boldLabel);
                
                Rect previewRect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                
                if (Event.current.type == EventType.Repaint)
                {
                    DrawFakePreview(previewRect);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a FocusPresetSO to preview.", MessageType.Info);
            }
        }

        private void DrawFakePreview(Rect rect)
        {
            // 1. Draw dark background
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

            var presetProp = serializedObject.FindProperty("_preset");
            FocusPresetSO preset = presetProp.objectReferenceValue as FocusPresetSO;
            if (preset == null) return;

            // 2. Draw Overlay
            if (preset.useDarkOverlay)
            {
                // EditorGUI.DrawRect doesn't blend alpha properly in older unity versions, 
                // but we can draw it and hope it blends, or use Handles
                Handles.DrawSolidRectangleWithOutline(rect, preset.overlayColor, Color.clear);
            }

            // 3. Draw Target
            Vector2 targetSize = new Vector2(100, 50);
            Rect targetRect = new Rect(rect.center.x - targetSize.x / 2, rect.center.y - targetSize.y / 2, targetSize.x, targetSize.y);
            
            EditorGUI.DrawRect(targetRect, Color.white);
            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            textStyle.normal.textColor = Color.black;
            GUI.Label(targetRect, "Dummy Button", textStyle);

            // 4. Indicator
            var overrideProp = serializedObject.FindProperty("_overridePresetLayout");
            bool isOverride = overrideProp != null && overrideProp.boolValue;

            IndicatorAnchor anchor;
            Vector2 customAnchor;
            float dirOffset;
            Vector2 posOffset;
            bool aimToAnchor;

            if (isOverride)
            {
                anchor = (IndicatorAnchor)serializedObject.FindProperty("_indicatorAnchor").enumValueIndex;
                customAnchor = serializedObject.FindProperty("_customIndicatorAnchor").vector2Value;
                dirOffset = serializedObject.FindProperty("_directionOffset").floatValue;
                posOffset = serializedObject.FindProperty("_indicatorOffset").vector2Value;
                aimToAnchor = serializedObject.FindProperty("_aimToAnchor").boolValue;
            }
            else
            {
                anchor = preset.indicatorAnchor;
                customAnchor = preset.customIndicatorAnchor;
                dirOffset = preset.directionOffset;
                posOffset = preset.indicatorOffset;
                aimToAnchor = preset.aimToAnchor;
            }

            Vector2 anchorPos = GetAnchorPosition(targetRect, anchor, customAnchor);
            
            Vector2 directionOutward = (anchorPos - targetRect.center).normalized;
            if (directionOutward == Vector2.zero) directionOutward = new Vector2(0, -1); // Up in IMGUI

            // Scale offsets proportionally for the preview (10f = preview equivalent of runtime's 20f * scaleFactor)
            float previewDirOffset = dirOffset * 10f;
            Vector2 previewPosOffset = posOffset * 10f;
            // In IMGUI, Y points down, so negate Y for preview to match Unity space
            previewPosOffset.y = -previewPosOffset.y; 
            
            Vector2 indicatorCenter = anchorPos + previewPosOffset + (directionOutward * previewDirOffset);

            float indSize = 32f;
            Rect indRect = new Rect(indicatorCenter.x - indSize / 2, indicatorCenter.y - indSize / 2, indSize, indSize);

            // Aim to anchor: rotate so indicator points from its position toward target center
            float angle = 0f;
            if (aimToAnchor)
            {
                Vector2 aimDirection = targetRect.center - indicatorCenter; // direction in IMGUI
                Vector2 unityAim = new Vector2(aimDirection.x, -aimDirection.y); // convert to Unity space
                angle = Vector2.SignedAngle(Vector2.up, unityAim);
            }

            Matrix4x4 oldMatrix = GUI.matrix;
            if (aimToAnchor)
            {
                // IMGUI RotateAroundPivot is clockwise. Unity Z rotation is counter-clockwise.
                // We negate the angle to match.
                GUIUtility.RotateAroundPivot(-angle, indicatorCenter);
            }

            Texture2D indTex = null;
            if (preset.indicatorPrefab != null)
            {
                var image = preset.indicatorPrefab.GetComponent<UnityEngine.UI.Image>();
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

        private Vector2 GetAnchorPosition(Rect targetRect, IndicatorAnchor anchor, Vector2 customAnchor)
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
