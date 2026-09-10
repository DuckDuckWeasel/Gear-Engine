using Scaffold.VisualScripting.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    [CustomPropertyDrawer(typeof(BlackboardDefinitionReference))]
    public sealed class BlackboardDefinitionReferenceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty sourceProperty = property.FindPropertyRelative("source");
            BlackboardDefinitionSource source =
                (BlackboardDefinitionSource)sourceProperty.enumValueIndex;
            float valueHeight = source == BlackboardDefinitionSource.Direct
                ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(
                    FindValueProperty(property, source),
                    true);
            return EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing +
                valueHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty sourceProperty = property.FindPropertyRelative("source");
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                Rect sourceRect = CreateSourceRect(position);
                EditorGUI.PropertyField(sourceRect, sourceProperty, label);
                DrawValue(
                    CreateValueRect(position, sourceRect),
                    property,
                    (BlackboardDefinitionSource)sourceProperty.enumValueIndex);
            }
            finally
            {
                EditorGUI.EndProperty();
            }
        }

        private void DrawValue(
            Rect position,
            SerializedProperty property,
            BlackboardDefinitionSource source)
        {
            EditorGUI.indentLevel++;
            try
            {
                if (source == BlackboardDefinitionSource.Direct)
                {
                    EditorGUI.LabelField(
                        position,
                        "Managed in the Blackboard window",
                        EditorStyles.miniLabel);
                    return;
                }

                GUIContent label = source == BlackboardDefinitionSource.ScriptableObject
                    ? new GUIContent("Definition Asset")
                    : new GUIContent("Definition Variable");
                EditorGUI.PropertyField(
                    position,
                    FindValueProperty(property, source),
                    label,
                    true);
            }
            finally
            {
                EditorGUI.indentLevel--;
            }
        }

        private SerializedProperty FindValueProperty(
            SerializedProperty property,
            BlackboardDefinitionSource source)
        {
            if (source == BlackboardDefinitionSource.Direct)
            {
                return property.FindPropertyRelative("directDefinition");
            }

            return source == BlackboardDefinitionSource.ScriptableObject
                ? property.FindPropertyRelative("definitionAsset")
                : property.FindPropertyRelative("variableId");
        }

        private Rect CreateSourceRect(Rect position)
        {
            return new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        }

        private Rect CreateValueRect(Rect position, Rect sourceRect)
        {
            float y = sourceRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            return new Rect(position.x, y, position.width, position.yMax - y);
        }
    }
}
