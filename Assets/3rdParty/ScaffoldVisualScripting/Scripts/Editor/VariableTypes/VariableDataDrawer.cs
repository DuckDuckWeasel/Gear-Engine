using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    public class VariableDataDrawer<T> : PropertyDrawer where T : Variable
    {
        private const float SourceButtonGap = 2f;

        private static readonly string[] SourceLabels = { "Direct", "Flowchart Variable", "ScriptableObject" };
        private static readonly VariableDataSource[] Sources =
        {
            VariableDataSource.Direct,
            VariableDataSource.FlowchartVariable,
            VariableDataSource.ScriptableObject,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            VariableInfoAttribute typeInfo = VariableEditor.GetVariableInfo(typeof(T));
            if (typeInfo == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            string propertyNameBase = ToPropertyNameBase(typeInfo);
            SerializedProperty referenceProperty = property.FindPropertyRelative(propertyNameBase + "Ref");
            SerializedProperty valueProperty = property.FindPropertyRelative(propertyNameBase + "Val");
            SerializedProperty sourceProperty = property.FindPropertyRelative("source");
            SerializedProperty scriptableObjectProperty = property.FindPropertyRelative(propertyNameBase + "SO");
            if (referenceProperty == null || valueProperty == null || sourceProperty == null || scriptableObjectProperty == null)
            {
                EditorGUI.HelpBox(position, "The value reference is missing a required serialized field.", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            VariableDataSource source = GetSource(sourceProperty, referenceProperty);
            float sourceButtonWidth = EditorGUIUtility.singleLineHeight;
            Rect valueRect = new Rect(position.x,
                                      position.y,
                                      position.width - sourceButtonWidth - SourceButtonGap,
                                      GetValueHeight(source, valueProperty, label));
            Rect sourceButtonRect = new Rect(valueRect.xMax + SourceButtonGap,
                                             position.y,
                                             sourceButtonWidth,
                                             EditorGUIUtility.singleLineHeight);

            DrawValue(valueRect, label, source, referenceProperty, valueProperty, scriptableObjectProperty);
            DrawSourceButton(sourceButtonRect, sourceProperty, source);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            VariableInfoAttribute typeInfo = VariableEditor.GetVariableInfo(typeof(T));
            if (typeInfo == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            string propertyNameBase = ToPropertyNameBase(typeInfo);
            SerializedProperty referenceProperty = property.FindPropertyRelative(propertyNameBase + "Ref");
            SerializedProperty valueProperty = property.FindPropertyRelative(propertyNameBase + "Val");
            SerializedProperty sourceProperty = property.FindPropertyRelative("source");
            SerializedProperty scriptableObjectProperty = property.FindPropertyRelative(propertyNameBase + "SO");
            if (referenceProperty == null || valueProperty == null || sourceProperty == null || scriptableObjectProperty == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            VariableDataSource source = GetSource(sourceProperty, referenceProperty);
            return GetValueHeight(source, valueProperty, label);
        }

        private static string ToPropertyNameBase(VariableInfoAttribute typeInfo)
        {
            string variableType = typeInfo.VariableType;
            return char.ToLowerInvariant(variableType[0]) + variableType.Substring(1);
        }

        private static VariableDataSource GetSource(SerializedProperty sourceProperty, SerializedProperty referenceProperty)
        {
            VariableDataSource source = (VariableDataSource)sourceProperty.enumValueIndex;
            return source == VariableDataSource.Unspecified
                ? referenceProperty.objectReferenceValue != null ? VariableDataSource.FlowchartVariable : VariableDataSource.Direct
                : source;
        }

        private static float GetValueHeight(VariableDataSource source, SerializedProperty valueProperty, GUIContent label)
        {
            return source == VariableDataSource.Direct
                ? EditorGUI.GetPropertyHeight(valueProperty, label, true)
                : EditorGUIUtility.singleLineHeight;
        }

        private static void DrawSourceButton(Rect rect, SerializedProperty sourceProperty, VariableDataSource source)
        {
            GUIContent buttonContent = new GUIContent("▾", "Value source: " + GetSourceLabel(source));
            if (GUI.Button(rect, buttonContent, EditorStyles.miniButton))
            {
                ShowSourceMenu(rect, sourceProperty, source);
            }
        }

        private static void ShowSourceMenu(Rect rect, SerializedProperty sourceProperty, VariableDataSource source)
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < Sources.Length; i++)
            {
                VariableDataSource menuSource = Sources[i];
                string menuLabel = SourceLabels[i];
                menu.AddItem(new GUIContent(menuLabel),
                             source == menuSource,
                             () => SetSource(sourceProperty, menuSource));
            }

            menu.DropDown(rect);
        }

        private static void SetSource(SerializedProperty sourceProperty, VariableDataSource source)
        {
            sourceProperty.enumValueIndex = (int)source;
            sourceProperty.serializedObject.ApplyModifiedProperties();
        }

        private static string GetSourceLabel(VariableDataSource source)
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                if (Sources[i] == source)
                {
                    return SourceLabels[i];
                }
            }

            return SourceLabels[0];
        }

        private static void DrawValue(Rect rect,
                                      GUIContent label,
                                      VariableDataSource source,
                                      SerializedProperty referenceProperty,
                                      SerializedProperty valueProperty,
                                      SerializedProperty scriptableObjectProperty)
        {
            switch (source)
            {
                case VariableDataSource.FlowchartVariable:
                    UnityEngine.Object selectedVariable = EditorGUI.ObjectField(rect,
                                                                                 label,
                                                                                 referenceProperty.objectReferenceValue,
                                                                                 typeof(T),
                                                                                 true);
                    referenceProperty.objectReferenceValue = selectedVariable;
                    break;
                case VariableDataSource.ScriptableObject:
                    EditorGUI.PropertyField(rect, scriptableObjectProperty, label);
                    break;
                case VariableDataSource.Direct:
                default:
                    CustomVariableDrawerLookup.DrawCustomOrPropertyField(typeof(T), rect, valueProperty, label);
                    break;
            }
        }
    }
}
