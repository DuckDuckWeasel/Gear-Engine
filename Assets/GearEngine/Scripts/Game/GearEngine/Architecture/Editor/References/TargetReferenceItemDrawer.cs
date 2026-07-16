using UnityEditor;
using UnityEngine;
using GearEngine.Core.Architecture.References;

namespace GearEngine.Core.Architecture.Editor.References
{
    [CustomPropertyDrawer(typeof(TargetReferenceItem))]
    public class TargetReferenceItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // We want to draw Type dropdown on the left, and the corresponding value field on the right
            Rect typeRect = new Rect(position.x, position.y, 110, position.height);
            Rect valueRect = new Rect(position.x + 115, position.y, position.width - 115, position.height);

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            TargetReferenceItemType type = (TargetReferenceItemType)typeProp.enumValueIndex;
            if (type == TargetReferenceItemType.DirectReference)
            {
                EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("directReference"), GUIContent.none);
            }
            else if (type == TargetReferenceItemType.GlobalVariable)
            {
                SerializedProperty globalVarProp = property.FindPropertyRelative("globalVariableName");
                
                var list = new System.Collections.Generic.List<string>();
                list.Add("<None>");

                if (TargetReferenceDrawer.GetGlobalVariableNames != null)
                {
                    string[] options = TargetReferenceDrawer.GetGlobalVariableNames();
                    if (options != null)
                    {
                        list.AddRange(options);
                    }
                }
                
                int currentIndex = list.IndexOf(globalVarProp.stringValue);
                if (currentIndex == -1) currentIndex = 0;
                
                int newIndex = EditorGUI.Popup(valueRect, currentIndex, list.ToArray());
                if (newIndex != currentIndex)
                {
                    globalVarProp.stringValue = newIndex == 0 ? "" : list[newIndex];
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
