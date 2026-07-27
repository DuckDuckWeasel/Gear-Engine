
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scaffold.EditorUtils
{
    [CustomEditor (typeof(Variable), true)]
    public class VariableEditor : CommandEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();

            Variable t = target as Variable;
            t.hideFlags = HideFlags.HideInInspector;
        }

        public static VariableInfoAttribute GetVariableInfo(System.Type variableType)
        {
            object[] attributes = variableType.GetCustomAttributes(typeof(VariableInfoAttribute), false);
            foreach (object obj in attributes)
            {
                VariableInfoAttribute variableInfoAttr = obj as VariableInfoAttribute;
                if (variableInfoAttr != null)
                {
                    return variableInfoAttr;
                }
            }
            
            return null;
        }

        public static void VariableField(SerializedProperty property, 
                                         GUIContent label, 
                                         Blackboard blackboard,
                                         string defaultText,
                                         Func<Variable, bool> filter, 
                                         Func<string, int, string[], int> drawer = null)
        {
            List<string> variableKeys = new List<string>();
            List<Variable> variableObjects = new List<Variable>();
            
            variableKeys.Add(defaultText);
            variableObjects.Add(null);
            
            List<Variable> variables = blackboard.Variables;
            int index = 0;
            int selectedIndex = 0;

            Variable selectedVariable = property.objectReferenceValue as Variable;

            // When there are multiple Blackboards in a scene with variables, switching
            // between the Blackboards can cause the wrong variable property
            // to be inspected for a single frame. This has the effect of causing private
            // variable references to be set to null when inspected. When this condition 
            // occurs we just skip displaying the property for this frame.
            if (selectedVariable != null &&
                selectedVariable.gameObject != blackboard.gameObject &&
                selectedVariable.Scope == VariableScope.Private)
            {
                property.objectReferenceValue = null;
                return;
            }

            foreach (Variable v in variables)
            {
                if (filter != null)
                {
                    if (!filter(v))
                    {
                        continue;
                    }
                }
                
                variableKeys.Add(v.Key);
                variableObjects.Add(v);
                
                index++;
                
                if (v == selectedVariable)
                {
                    selectedIndex = index;
                }
            }

            List<Blackboard> fsList = Blackboard.CachedBlackboards;
            foreach (Blackboard fs in fsList)
            {
                if (fs == blackboard)
                {
                    continue;
                }

                List<Variable> publicVars = fs.GetPublicVariables();
                foreach (Variable v in publicVars)
                {
                    if (filter != null)
                    {
                        if (!filter(v))
                        {
                            continue;
                        }
                    }

                    variableKeys.Add(fs.name + "/" + v.Key);
                    variableObjects.Add(v);

                    index++;

                    if (v == selectedVariable)
                    {
                        selectedIndex = index;
                    }
                }
            }

            if (drawer == null)
            {
                selectedIndex = EditorGUILayout.Popup(label.text, selectedIndex, variableKeys.ToArray());
            }
            else
            {
                selectedIndex = drawer(label.text, selectedIndex, variableKeys.ToArray());
            }

            property.objectReferenceValue = variableObjects[selectedIndex];
        }
    }

    [CustomPropertyDrawer(typeof(VariablePropertyAttribute))]
    public class VariableDrawer : PropertyDrawer
    {   
        
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
        {
            VariablePropertyAttribute variableProperty = attribute as VariablePropertyAttribute;
            if (variableProperty == null)
            {
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Filter the variables by the types listed in the VariableProperty attribute
            Func<Variable, bool> compare = v => 
            {
                if (v == null)
                {
                    return false;
                }

                if (variableProperty.VariableTypes.Length == 0)
                {
                    var compatChecker = property.serializedObject.targetObject as ICollectionCompatible;
                    if (compatChecker != null)
                    {
                        return compatChecker.IsVarCompatibleWithCollection(v, variableProperty.compatibleVariableName);
                    }
                    else
                    {
                        return true;
                    }
                }

                return variableProperty.VariableTypes.Contains<System.Type>(v.GetType());
            };

            VariableEditor.VariableField(property, 
                                         label,
                                         BlackboardWindow.GetBlackboard(),
                                         variableProperty.defaultText,
                                         compare,
                                         (s,t,u) => (EditorGUI.Popup(position, s, t, u)));

            EditorGUI.EndProperty();
        }
    }
    
}
