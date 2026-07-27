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
            SerializedProperty valueProperty = FindValueProperty(property, sourceProperty);
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(valueProperty, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty sourceProperty = property.FindPropertyRelative("source");
            Rect sourceRect = CreateSourceRect(position);
            EditorGUI.PropertyField(sourceRect, sourceProperty, label);
            Rect valueRect = CreateValueRect(position, sourceRect);
            EditorGUI.PropertyField(valueRect, FindValueProperty(property, sourceProperty), true);
        }

        private SerializedProperty FindValueProperty(SerializedProperty property, SerializedProperty sourceProperty)
        {
            BlackboardDefinitionSource source = (BlackboardDefinitionSource)sourceProperty.enumValueIndex;
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
