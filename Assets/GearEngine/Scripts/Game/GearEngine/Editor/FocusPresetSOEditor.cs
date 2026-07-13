using UnityEditor;
using UnityEngine;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;

namespace GearEngine.GearEngine.Editor
{
    /// <summary>
    /// Custom Editor for FocusPresetSO.
    /// Replaces the Odin [OnInspectorGUI] button that was previously embedded in the Runtime class.
    /// All Editor-only UI belongs here, keeping the Runtime asset clean.
    /// </summary>
    [CustomEditor(typeof(FocusPresetSO))]
    public class FocusPresetSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FocusPresetSO preset = (FocusPresetSO)target;

            EditorGUILayout.Space(15);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(10);
            GUILayout.Label("Preview & Actions", EditorStyles.boldLabel);

            if (preset.useUIEffect && preset.uiEffectPreset != null)
            {
                Coffee.UIEffects.UIEffectPreset p = preset.uiEffectPreset;
                string text = "<b>Active Effects:</b>\n";
                if (p.m_ToneFilter != Coffee.UIEffects.ToneFilter.None) text += $"- Tone: {p.m_ToneFilter}\n";
                if (p.m_ColorFilter != Coffee.UIEffects.ColorFilter.None) text += $"- Color: {p.m_ColorFilter}\n";
                if (p.m_SamplingFilter != Coffee.UIEffects.SamplingFilter.None) text += $"- Filter: {p.m_SamplingFilter}\n";
                EditorGUILayout.HelpBox(text, MessageType.Info);
                EditorGUILayout.HelpBox("Configure visually by clicking 'Open Live Preview'.", MessageType.Info);
            }

            if (GUILayout.Button("Open Live Preview Window", GUILayout.Height(35)))
            {
                System.Type windowType = System.Type.GetType("GearEngine.GearEngine.Editor.TutorialFocusPreviewWindow, Game.GearEngine.Editor");
                if (windowType != null)
                {
                    EditorWindow window = EditorWindow.GetWindow(windowType);
                    window.titleContent = new GUIContent("Focus Preview");
                    System.Reflection.MethodInfo method = windowType.GetMethod("SetPreset");
                    if (method != null)
                    {
                        method.Invoke(window, new object[] { preset });
                    }
                }
                else
                {
                    Debug.LogError("Could not find TutorialFocusPreviewWindow.");
                }
            }
        }
    }
}
