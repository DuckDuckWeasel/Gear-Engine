using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace GearEngine.GearEngine.Editor.Tags
{
    [CustomPropertyDrawer(typeof(UnityNativeTagMask))]
    public class UnityNativeTagMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty tagsProp = property.FindPropertyRelative("tags");

            // Build a list of all native tags available in the project
            string[] allTags = InternalEditorUtility.tags;

            // Compute current mask from the saved strings
            int mask = 0;
            if (tagsProp != null)
            {
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    string tagStr = tagsProp.GetArrayElementAtIndex(i).stringValue;
                    int index = System.Array.IndexOf(allTags, tagStr);
                    if (index >= 0)
                    {
                        mask |= (1 << index);
                    }
                }
            }

            // Draw the Mask Field
            EditorGUI.BeginChangeCheck();
            int newMask = EditorGUI.MaskField(position, label, mask, allTags);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Convert mask back to string list
                List<string> selectedTags = new List<string>();
                for (int i = 0; i < allTags.Length; i++)
                {
                    if ((newMask & (1 << i)) != 0)
                    {
                        selectedTags.Add(allTags[i]);
                    }
                }

                tagsProp.ClearArray();
                for (int i = 0; i < selectedTags.Count; i++)
                {
                    tagsProp.InsertArrayElementAtIndex(i);
                    tagsProp.GetArrayElementAtIndex(i).stringValue = selectedTags[i];
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
