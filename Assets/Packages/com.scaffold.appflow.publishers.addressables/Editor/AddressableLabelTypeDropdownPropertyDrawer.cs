using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables.Editor
{
    /// <summary>Popup for an AQN string narrowed to types that actually appear among entries of a sibling <see cref="AssetLabelReference"/> field.</summary>
    [CustomPropertyDrawer(typeof(AddressableLabelTypeDropdownAttribute))]
    public sealed class AddressableLabelTypeDropdownPropertyDrawer : PropertyDrawer
    {
        private const string NoLabelLabel = "— (no label) —";
        private const string NoEntriesLabel = "— (no entries with label) —";
        private const string PickLabel = "— (pick) —";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var attr = (AddressableLabelTypeDropdownAttribute)attribute;
            EditorGUI.BeginProperty(position, label, property);
            DrawPopup(position, property, label, attr);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static void DrawPopup(Rect position, SerializedProperty property, GUIContent label, AddressableLabelTypeDropdownAttribute attr)
        {
            string labelString = ReadSiblingLabelString(property, attr.SiblingLabelFieldName);
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect valueRect = EditorGUI.PrefixLabel(line, label);

            if (string.IsNullOrEmpty(labelString))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Popup(valueRect, 0, new[] { NoLabelLabel });
                }

                if (!string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = string.Empty;
                }

                return;
            }

            IReadOnlyList<Type> types = AddressableBakeUtility.CollectLabelEntryTypes(labelString);
            if (types.Count == 0)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Popup(valueRect, 0, new[] { NoEntriesLabel });
                }

                if (!string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = string.Empty;
                }

                return;
            }

            // Single type: store it implicitly so the bake works without user input.
            if (types.Count == 1)
            {
                string aqn = types[0].AssemblyQualifiedName ?? string.Empty;
                if (property.stringValue != aqn)
                {
                    property.stringValue = aqn;
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Popup(valueRect, 0, new[] { BuildRowLabel(types[0]) });
                }

                return;
            }

            string[] labels = BuildLabels(types);
            string[] aqns = BuildAqns(types);
            int selected = ResolveSelectedIndex(property.stringValue, aqns);
            EditorGUI.BeginChangeCheck();
            int next = EditorGUI.Popup(valueRect, selected, labels);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = next <= 0 ? string.Empty : aqns[next - 1];
            }
        }

        private static int ResolveSelectedIndex(string current, string[] aqns)
        {
            if (string.IsNullOrEmpty(current))
            {
                return 0;
            }

            int i = Array.IndexOf(aqns, current);
            return i < 0 ? 0 : i + 1;
        }

        private static string[] BuildLabels(IReadOnlyList<Type> types)
        {
            var arr = new string[types.Count + 1];
            arr[0] = PickLabel;
            for (int i = 0; i < types.Count; i++)
            {
                arr[i + 1] = BuildRowLabel(types[i]);
            }

            return arr;
        }

        private static string[] BuildAqns(IReadOnlyList<Type> types)
        {
            var arr = new string[types.Count];
            for (int i = 0; i < types.Count; i++)
            {
                arr[i] = types[i].AssemblyQualifiedName ?? string.Empty;
            }

            return arr;
        }

        private static string BuildRowLabel(Type t)
        {
            string an = t.Assembly.GetName().Name;
            if (!string.IsNullOrEmpty(t.Namespace))
            {
                return $"{t.Namespace}.{t.Name}  ({an})";
            }

            return $"{t.Name}  ({an})";
        }

        private static string ReadSiblingLabelString(SerializedProperty property, string siblingFieldName)
        {
            if (string.IsNullOrEmpty(siblingFieldName))
            {
                return null;
            }

            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            string parentPath = lastDot < 0 ? string.Empty : path.Substring(0, lastDot);
            string siblingPath = string.IsNullOrEmpty(parentPath) ? siblingFieldName : parentPath + "." + siblingFieldName;
            SerializedProperty sibling = property.serializedObject.FindProperty(siblingPath);
            if (sibling == null)
            {
                return null;
            }

            SerializedProperty labelStringProp = sibling.FindPropertyRelative("m_LabelString");
            if (labelStringProp == null || labelStringProp.propertyType != SerializedPropertyType.String)
            {
                labelStringProp = sibling.FindPropertyRelative("labelString");
            }

            return labelStringProp != null ? labelStringProp.stringValue : null;
        }
    }
}
