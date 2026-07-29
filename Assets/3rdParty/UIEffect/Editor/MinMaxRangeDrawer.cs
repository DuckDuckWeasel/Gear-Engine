using UnityEditor;
using UnityEngine;

namespace Coffee.UIEffectInternal
{
    [CustomPropertyDrawer(typeof(MinMax01))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        private const float k_NumWidth = 50;
        private const float k_Space = 5;

        private static bool IsSingleLine(GUIContent label)
        {
            return EditorGUIUtility.wideMode || label == null || string.IsNullOrEmpty(label.text);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return IsSingleLine(label) ? 18 : 36;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty minProperty = property.FindPropertyRelative("m_Min");
            SerializedProperty maxProperty = property.FindPropertyRelative("m_Max");

            if (IsSingleLine(label))
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            }
            else
            {
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                float indent = (EditorGUI.indentLevel + 1) * 15f;
                position = new Rect(position.x + indent, position.y + 18, position.width - indent, 16);
            }

            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;
            if (Draw(position, ref min, ref max))
            {
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
            }

            EditorGUI.EndProperty();
        }

        public static bool Draw(Rect position, ref float minValue, ref float maxValue)
        {
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();

            Rect rect = new Rect(position.x, position.y, k_NumWidth, position.height);
            minValue = Mathf.Clamp(EditorGUI.FloatField(rect, minValue), 0, maxValue);

            rect.x += rect.width + k_Space;
            rect.width = position.width - k_NumWidth * 2 - k_Space * 2;
            EditorGUI.MinMaxSlider(rect, ref minValue, ref maxValue, 0, 1);

            rect.x += rect.width + k_Space;
            rect.width = k_NumWidth;
            maxValue = Mathf.Clamp(EditorGUI.FloatField(rect, maxValue), minValue, 1);

            EditorGUI.indentLevel = indentLevel;
            return EditorGUI.EndChangeCheck();
        }

        public static bool DrawLayout(GUIContent label, ref float minValue, ref float maxValue)
        {
            Rect position;
            if (IsSingleLine(label))
            {
                position = EditorGUILayout.GetControlRect(true, 18f);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            }
            else
            {
                position = EditorGUILayout.GetControlRect(true, 36f);
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                float indent = (EditorGUI.indentLevel + 1) * 15f;
                position = new Rect(position.x + indent, position.y + 18, position.width - indent, 16);
            }

            return Draw(position, ref minValue, ref maxValue);
        }
    }
}
