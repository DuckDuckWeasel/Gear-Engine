
using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Custom drawer for the AnyVaraibleAndDataPair, shows only the matching data for the targeted variable
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(Scaffold.AnyVariableAndDataPair))]
    public class AnyVariableAndDataPairDrawer : PropertyDrawer
    {
        public Scaffold.Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var varProp = property.FindPropertyRelative("variable");
            float variableHeight = EditorGUI.GetPropertyHeight(varProp);
            Rect variableRect = position;
            variableRect.height = variableHeight;

            EditorGUI.PropertyField(variableRect, varProp, label);

            position.y = variableRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            if (varProp.objectReferenceValue != null)
            {
                var varPropType = varProp.objectReferenceValue.GetType();

                var typeActionsRes = AnyVariableAndDataPair.typeActionLookup[varPropType];

                if (typeActionsRes != null)
                {
                    var targetName = "data." + typeActionsRes.DataPropName;
                    var dataProp = property.FindPropertyRelative(targetName);
                    if (dataProp != null)
                    {
                        EditorGUI.PropertyField(position, dataProp, new GUIContent("Data", "Data to use in pair with the above variable."));
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Cound not find property in AnyVariableData of name " + targetName);
                    }
                }
                else
                {
                    //no matching data type, oops
                    EditorGUI.LabelField(position, "Cound not find property in AnyVariableData of type " + varPropType.Name);
                }
            }
            else
            {
                //no var selected
                EditorGUI.LabelField(position, "Must select a variable before setting data.");
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //changes in new Unity circa UIElements mean that some data that used to be single line
            //  are now multiple lines, so we have to ask the props individually how high they are
            var dataProp = GetDataProp(property);

            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("variable")) +
                EditorGUIUtility.standardVerticalSpacing +
                (dataProp != null ? 
                    EditorGUI.GetPropertyHeight(dataProp) :
                    EditorGUIUtility.singleLineHeight);
        }

        protected SerializedProperty GetDataProp(SerializedProperty property)
        {
            var varProp = property.FindPropertyRelative("variable");
            if (varProp.objectReferenceValue != null)
            {
                var varPropType = varProp.objectReferenceValue.GetType();

                var typeActionsRes = AnyVariableAndDataPair.typeActionLookup[varPropType];

                if (typeActionsRes != null)
                {
                    var targetName = "data." + typeActionsRes.DataPropName;
                    return property.FindPropertyRelative(targetName);
                }
            }
            return null;
        }
    }
}
