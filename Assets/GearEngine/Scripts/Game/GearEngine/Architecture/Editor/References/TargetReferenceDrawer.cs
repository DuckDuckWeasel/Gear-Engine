using UnityEditor;
using UnityEngine;
using GearEngine.Core.Architecture.References;

namespace GearEngine.Core.Architecture.Editor.References
{
    [CustomPropertyDrawer(typeof(TargetReference))]
    public class TargetReferenceDrawer : PropertyDrawer
    {
        /// <summary>
        /// External editor modules (like Fungus integration) can inject a method here 
        /// to provide a list of available global variables for the dropdown.
        /// </summary>
        public static System.Func<string[]> GetGlobalVariableNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            int oldIndent = EditorGUI.indentLevel;
            // The position passed by Unity is already indented. Resetting indentLevel prevents 
            // EditorGUI.Foldout and PropertyField from double-indenting and overlapping.
            EditorGUI.indentLevel = 0;

            // Draw foldout
            float strategyWidth = Mathf.Min(position.width * 0.45f, 160f); // Max 160px for the dropdown
            Rect foldoutRect = new Rect(position.x, position.y, position.width - strategyWidth - 5f, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            // Draw Strategy dropdown on the same line, on the far right
            Rect strategyRect = new Rect(position.x + position.width - strategyWidth, position.y, strategyWidth, EditorGUIUtility.singleLineHeight);
            SerializedProperty strategyProp = property.FindPropertyRelative("strategy");
            EditorGUI.PropertyField(strategyRect, strategyProp, GUIContent.none);

            if (property.isExpanded)
            {
                // Manually indent children
                float childX = position.x + 15f;
                float childWidth = position.width - 15f;

                Rect contentRect = new Rect(childX, position.y + EditorGUIUtility.singleLineHeight + 2, childWidth, EditorGUIUtility.singleLineHeight);

                TargetResolutionStrategy strategy = (TargetResolutionStrategy)strategyProp.enumValueIndex;

                switch (strategy)
                {
                    case TargetResolutionStrategy.DirectReference:
                        EditorGUI.PropertyField(contentRect, property.FindPropertyRelative("directReference"));
                        break;
                    case TargetResolutionStrategy.Tags:
                        SerializedProperty tagFilterProp = property.FindPropertyRelative("tagFilter");
                        EditorGUI.PropertyField(contentRect, tagFilterProp, true); // true to draw children
                        break;
                    case TargetResolutionStrategy.GlobalVariable:
                        SerializedProperty globalVarProp = property.FindPropertyRelative("globalVariableName");
                        
                        var list = new System.Collections.Generic.List<string>();
                        list.Add("<None>");

                        if (GetGlobalVariableNames != null)
                        {
                            string[] options = GetGlobalVariableNames();
                            if (options != null)
                            {
                                list.AddRange(options);
                            }
                        }
                        
                        int currentIndex = list.IndexOf(globalVarProp.stringValue);
                        if (currentIndex == -1) currentIndex = 0; // fallback to <None>
                        
                        int newIndex = EditorGUI.Popup(contentRect, globalVarProp.displayName, currentIndex, list.ToArray());
                        if (newIndex != currentIndex)
                        {
                            globalVarProp.stringValue = newIndex == 0 ? "" : list[newIndex];
                        }
                        break;
                    case TargetResolutionStrategy.MultipleReferences:
                        SerializedProperty referencesProp = property.FindPropertyRelative("references");
                        EditorGUI.PropertyField(contentRect, referencesProp, true);
                        break;
                }
            }

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight + 2; // Foldout line only

            SerializedProperty strategyProp = property.FindPropertyRelative("strategy");
            TargetResolutionStrategy strategy = (TargetResolutionStrategy)strategyProp.enumValueIndex;

            switch (strategy)
            {
                case TargetResolutionStrategy.DirectReference:
                case TargetResolutionStrategy.GlobalVariable:
                    height += EditorGUIUtility.singleLineHeight + 2;
                    break;
                case TargetResolutionStrategy.MultipleReferences:
                    SerializedProperty referencesProp = property.FindPropertyRelative("references");
                    height += EditorGUI.GetPropertyHeight(referencesProp, true) + 2;
                    break;
                case TargetResolutionStrategy.Tags:
                    SerializedProperty tagFilterProp = property.FindPropertyRelative("tagFilter");
                    height += EditorGUI.GetPropertyHeight(tagFilterProp, true) + 2;
                    break;
            }

            return height;
        }
    }
}
