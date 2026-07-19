
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Custom drawer for the BlockReference, allows for more easily selecting a target block in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(Scaffold.BlockReference))]
    public class BlockReferenceDrawer : PropertyDrawer
    {
        public Scaffold.Blackboard lastBlackboard;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var l = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, l);
            position.height = EditorGUIUtility.singleLineHeight;
            var block = property.FindPropertyRelative("block");

            Scaffold.Block b = block.objectReferenceValue as Scaffold.Block;

            if (block.objectReferenceValue != null && lastBlackboard == null)
            {
                if (b != null)
                {
                    lastBlackboard = b.GetBlackboard();
                }
            }

            lastBlackboard = EditorGUI.ObjectField(position, lastBlackboard, typeof(Scaffold.Blackboard), true) as Scaffold.Blackboard;
            position.y += EditorGUIUtility.singleLineHeight;
            if (lastBlackboard != null)
                b = Scaffold.EditorUtils.BlockEditor.BlockField(position, new GUIContent("None"), lastBlackboard, b);
            else
                EditorGUI.PrefixLabel(position, new GUIContent("Blackboard Required"));

            block.objectReferenceValue = b;

            block.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
}