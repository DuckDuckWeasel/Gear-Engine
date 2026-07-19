
using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Custom drawer for the VariableReference, allows for more easily selecting a target variable in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(Scaffold.VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public Scaffold.Blackboard lastBlackboard;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var l = EditorGUI.BeginProperty(position, label, property);
            var startPos = position;
            position = EditorGUI.PrefixLabel(position, l);
            position.height = EditorGUIUtility.singleLineHeight;
            var variable = property.FindPropertyRelative("variable");

            Scaffold.Variable v = variable.objectReferenceValue as Scaffold.Variable;

            if (variable.objectReferenceValue != null && lastBlackboard == null)
            {
                if (v != null)
                {
                    lastBlackboard = v.GetComponent<Blackboard>();
                }
            }

            lastBlackboard = EditorGUI.ObjectField(position, lastBlackboard, typeof(Scaffold.Blackboard), true) as Scaffold.Blackboard;
            position.y += EditorGUIUtility.singleLineHeight;
            if (lastBlackboard != null)
            {
                var ourPos = startPos;
                ourPos.y = position.y;
                var prefixLabel = new GUIContent(v != null ? v.GetType().Name : "No Var Selected");
                EditorGUI.indentLevel++;
                VariableEditor.VariableField(variable,
                                             prefixLabel,
                                             lastBlackboard,
                                             "<None>",
                                             null,
                                             //lable, index, elements
                                             (s, t, u) => (EditorGUI.Popup(ourPos, s, t, u)));


                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.PrefixLabel(position, new GUIContent("Blackboard Required"));
            }

            variable.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
}