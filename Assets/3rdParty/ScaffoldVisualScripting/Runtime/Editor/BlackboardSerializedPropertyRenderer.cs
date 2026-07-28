using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardSerializedPropertyRenderer
    {
        private static readonly HashSet<string> s_expandedProperties = new HashSet<string>();

        public static bool DrawManagedReference(UnityEngine.Object owner, object target, IAction action = null)
        {
            return DrawManagedReference(owner, target, Array.Empty<string>(), action, null);
        }

        public static bool DrawManagedReference(UnityEngine.Object owner, object target, IReadOnlyCollection<string> hiddenNames, IAction action = null)
        {
            return DrawManagedReference(owner, target, hiddenNames, action, null);
        }

        public static bool DrawManagedReference(UnityEngine.Object owner, object target, IReadOnlyCollection<string> hiddenNames, IAction action, BlackboardDefinition definition)
        {
            SerializedObject serialized = new SerializedObject(owner);
            serialized.Update();
            SerializedProperty root = FindManagedReference(serialized, target);
            if (root == null)
            {
                EditorGUILayout.HelpBox("The selected managed value could not be located in its authoring owner.", MessageType.Warning);
                return false;
            }

            EditorGUI.BeginChangeCheck();
            DrawChildren(owner, target, root, hiddenNames, action, definition);
            bool guiChanged = EditorGUI.EndChangeCheck();
            if (!guiChanged)
            {
                return false;
            }

            return serialized.ApplyModifiedProperties();
        }

        public static SerializedProperty FindManagedReference(SerializedObject serialized, object target)
        {
            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.Next(enterChildren))
            {
                enterChildren = true;
                if (iterator.propertyType == SerializedPropertyType.ManagedReference && ReferenceEquals(iterator.managedReferenceValue, target))
                {
                    return iterator.Copy();
                }
            }

            return null;
        }

        public static void ApplyExpandedState(UnityEngine.Object owner, object target, SerializedProperty property)
        {
            if (owner == null || target == null || property == null || !property.hasVisibleChildren)
            {
                return;
            }

            property.isExpanded = s_expandedProperties.Contains(GetExpansionKey(owner, target, property));
        }

        public static void CaptureExpandedState(UnityEngine.Object owner, object target, SerializedProperty property)
        {
            if (owner == null || target == null || property == null || !property.hasVisibleChildren)
            {
                return;
            }

            string key = GetExpansionKey(owner, target, property);
            if (property.isExpanded)
            {
                s_expandedProperties.Add(key);
            }
            else
            {
                s_expandedProperties.Remove(key);
            }
        }

        private static void DrawChildren(UnityEngine.Object owner, object target, SerializedProperty root, IReadOnlyCollection<string> hiddenNames, IAction action, BlackboardDefinition definition)
        {
            SerializedProperty current = root.Copy();
            SerializedProperty end = root.GetEndProperty();
            bool enterChildren = true;
            while (current.NextVisible(enterChildren) && !SerializedProperty.EqualContents(current, end))
            {
                enterChildren = false;
                if (current.depth == root.depth + 1 && ShouldDraw(current, hiddenNames, action))
                {
                    DrawProperty(owner, target, current, action, definition);
                }
            }
        }

        private static void DrawProperty(UnityEngine.Object owner, object target, SerializedProperty property, IAction action, BlackboardDefinition definition)
        {
            if (definition != null && action is IBlockConnectionSource && property.name == "targetBlockName")
            {
                DrawBlockReference(property, definition);
                return;
            }

            if (definition != null && property.type == nameof(VariableReference))
            {
                DrawVariableReference(property, definition);
                return;
            }

            ApplyExpandedState(owner, target, property);
            using (BlackboardCompatibilityVariableDataDrawer.UseDefinition(definition))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            CaptureExpandedState(owner, target, property);
        }

        private static string GetExpansionKey(UnityEngine.Object owner, object target, SerializedProperty property)
        {
            return $"{owner.GetEntityId()}:{RuntimeHelpers.GetHashCode(target)}:{property.name}";
        }

        private static void DrawBlockReference(SerializedProperty property, BlackboardDefinition definition)
        {
            SerializedProperty value = property.FindPropertyRelative("stringVal");
            SerializedProperty source = property.FindPropertyRelative("source");
            if (value == null || source == null)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            string[] choices = new string[definition.Blocks.Count + 1];
            choices[0] = "None";
            int selected = 0;
            for (int index = 0; index < definition.Blocks.Count; index++)
            {
                BlockDefinition block = definition.Blocks[index];
                choices[index + 1] = block?.Name ?? "Missing Block";
                if (block != null && string.Equals(block.Name, value.stringValue, StringComparison.Ordinal))
                {
                    selected = index + 1;
                }
            }

            int next = EditorGUILayout.Popup(property.displayName, selected, choices);
            if (next != selected)
            {
                value.stringValue = next == 0 ? string.Empty : choices[next];
                source.enumValueIndex = (int)Scaffold.VariableDataSource.Direct;
            }
        }

        private static void DrawVariableReference(SerializedProperty property, BlackboardDefinition definition)
        {
            SerializedProperty scope = property.FindPropertyRelative("scope");
            SerializedProperty id = property.FindPropertyRelative("definitionId")?.FindPropertyRelative("value");
            if (scope == null || id == null)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            EditorGUILayout.PropertyField(scope);
            List<VariableDefinitionBase> compatible = new List<VariableDefinitionBase>();
            for (int index = 0; index < definition.Variables.Count; index++)
            {
                VariableDefinitionBase variable = definition.Variables[index];
                if (variable != null && (int)variable.Scope == scope.enumValueIndex)
                {
                    compatible.Add(variable);
                }
            }

            string[] choices = new string[compatible.Count + 1];
            choices[0] = "None";
            int selected = 0;
            for (int index = 0; index < compatible.Count; index++)
            {
                choices[index + 1] = compatible[index].Key;
                if (compatible[index].DefinitionId.Value == id.stringValue)
                {
                    selected = index + 1;
                }
            }

            int next = EditorGUILayout.Popup(property.displayName, selected, choices);
            if (next != selected)
            {
                id.stringValue = next == 0 ? string.Empty : compatible[next - 1].DefinitionId.Value;
            }
            else if (selected == 0 && !string.IsNullOrWhiteSpace(id.stringValue))
            {
                EditorGUILayout.HelpBox("The selected variable no longer exists in this Blackboard scope.", MessageType.Warning);
            }
        }

        private static bool ShouldDraw(SerializedProperty property, IReadOnlyCollection<string> hiddenNames, IAction action)
        {
            if (property.name == "definitionId" || hiddenNames.Contains(property.name))
            {
                return false;
            }

            return action == null || BlackboardEditorDisplay.IsPropertyVisible(action, property.name);
        }
    }
}
