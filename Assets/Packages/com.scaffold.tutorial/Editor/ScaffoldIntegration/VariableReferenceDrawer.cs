using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Scaffold.Tutorial.Variables;

namespace Scaffold.Tutorial.Editor
{
    /// <summary>
    /// Generic PropertyDrawer for TutorialVariableReference<T1,T2>.
    ///
    /// Layout:
    ///   Row 0: [Label] [Constant | Reference toolbar]
    ///   Constant mode:
    ///     - If data is null: [Type picker button spanning full width]
    ///     - If data is set:  [Type label + clear X] then sub-fields of the concrete type
    ///   Reference mode:
    ///     Row 1: [ObjectField for the SO reference]
    ///
    /// Uses only Unity native IMGUI APIs. No Odin, no Tri-Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(TutorialVariableReference<,>), true)]
    public class TutorialVariableReferenceDrawer : PropertyDrawer
    {
        private const float ToolbarWidth = 160f;
        private const float LineHeight = 20f;
        private const float Spacing = 2f;

        // ──────────────────────────────────────────
        // Height calculation
        // ──────────────────────────────────────────

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty sourceProp = property.FindPropertyRelative("source");
            if (sourceProp == null) return LineHeight;

            // Row 0: toolbar row
            float total = LineHeight + Spacing;

            bool isConstant = sourceProp.enumValueIndex == (int)VariableSource.Constant;
            if (isConstant)
            {
                SerializedProperty dataProp = property.FindPropertyRelative("data");
                if (dataProp == null) return total;

                bool hasValue = dataProp.managedReferenceValue != null;
                if (!hasValue)
                {
                    // Just the type picker row
                    total += LineHeight;
                }
                else
                {
                    // Type header row
                    total += LineHeight + Spacing;
                    // Each child field
                    foreach (SerializedProperty child in GetDirectChildren(dataProp))
                    {
                        total += EditorGUI.GetPropertyHeight(child, true) + Spacing;
                    }
                }
            }
            else
            {
                // Reference: just an ObjectField row
                total += LineHeight;
            }

            return total;
        }

        // ──────────────────────────────────────────
        // Drawing
        // ──────────────────────────────────────────

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sourceProp = property.FindPropertyRelative("source");
            if (sourceProp == null)
            {
                EditorGUI.LabelField(position, label.text, "(source field not found)");
                EditorGUI.EndProperty();
                return;
            }

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // ── Row 0: label + source toolbar ──────────────────
            Rect row0 = new Rect(position.x, position.y, position.width, LineHeight);

            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(row0.x, row0.y, labelWidth, row0.height);
            Rect toolbarRect = new Rect(row0.x + labelWidth, row0.y, ToolbarWidth, row0.height);

            EditorGUI.LabelField(labelRect, label);

            string[] options = { "Constant", "Reference" };
            int currentSource = sourceProp.enumValueIndex;
            int newSource = GUI.Toolbar(toolbarRect, currentSource, options, EditorStyles.miniButton);
            if (newSource != currentSource)
            {
                sourceProp.enumValueIndex = newSource;
                property.serializedObject.ApplyModifiedProperties();
            }

            float y = row0.yMax + Spacing;
            bool isConstant = sourceProp.enumValueIndex == (int)VariableSource.Constant;

            if (isConstant)
            {
                DrawConstantMode(property, position, ref y);
            }
            else
            {
                DrawReferenceMode(property, position, ref y);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        // ── Constant mode ──────────────────────────────────────

        private void DrawConstantMode(SerializedProperty property, Rect position, ref float y)
        {
            SerializedProperty dataProp = property.FindPropertyRelative("data");
            if (dataProp == null)
            {
                DrawBox(new Rect(position.x, y, position.width, LineHeight),
                    "(data field not found — check [SerializeReference] on TutorialVariableReference)", true);
                return;
            }

            bool hasValue = dataProp.managedReferenceValue != null;

            if (!hasValue)
            {
                // Show type picker
                Rect pickerRect = new Rect(position.x, y, position.width, LineHeight);
                string fieldTypeName = GetManagedReferenceFieldTypeName(dataProp);
                string buttonLabel = string.IsNullOrEmpty(fieldTypeName)
                    ? "＋ Select Type..."
                    : $"＋ Select {fieldTypeName}...";

                if (GUI.Button(pickerRect, buttonLabel, EditorStyles.miniButton))
                {
                    ShowTypePicker(dataProp, GetConcreteTypes(dataProp));
                }
                y += LineHeight;
            }
            else
            {
                // Type header bar with Clear button
                Rect headerRect = new Rect(position.x, y, position.width, LineHeight);
                string typeName = dataProp.managedReferenceValue.GetType().Name;

                DrawTypeHeader(headerRect, typeName, () =>
                {
                    dataProp.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });
                y += LineHeight + Spacing;

                // Draw each field of the concrete instance
                EditorGUI.indentLevel++;
                foreach (SerializedProperty child in GetDirectChildren(dataProp))
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(position.x, y, position.width, childHeight);
                    EditorGUI.PropertyField(childRect, child, true);
                    y += childHeight + Spacing;
                }
                EditorGUI.indentLevel--;
            }
        }

        // ── Reference mode ─────────────────────────────────────

        private void DrawReferenceMode(SerializedProperty property, Rect position, ref float y)
        {
            SerializedProperty refProp = property.FindPropertyRelative("reference");
            if (refProp == null) return;

            Rect refRect = new Rect(position.x, y, position.width, LineHeight);
            EditorGUI.PropertyField(refRect, refProp, GUIContent.none);
            y += LineHeight;
        }

        // ── Helpers ────────────────────────────────────────────

        private void DrawTypeHeader(Rect rect, string typeName, Action onClear)
        {
            Color bg = EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.35f, 0.5f, 1f)
                : new Color(0.6f, 0.75f, 0.95f, 1f);
            EditorGUI.DrawRect(rect, bg);

            Rect clearRect = new Rect(rect.xMax - 22f, rect.y + 1f, 20f, rect.height - 2f);
            Rect labelRect = new Rect(rect.x + 6f, rect.y, rect.width - 30f, rect.height);

            EditorGUI.LabelField(labelRect, typeName, EditorStyles.boldLabel);

            if (GUI.Button(clearRect, "✕", EditorStyles.miniLabel))
            {
                onClear?.Invoke();
            }
        }

        private void DrawBox(Rect rect, string message, bool isError)
        {
            EditorGUI.HelpBox(rect, message, isError ? MessageType.Error : MessageType.Info);
        }

        private void ShowTypePicker(SerializedProperty dataProp, List<Type> types)
        {
            GenericMenu menu = new GenericMenu();

            if (types.Count == 0)
            {
                string fieldTypeName = GetManagedReferenceFieldTypeName(dataProp);
                menu.AddDisabledItem(new GUIContent($"No {fieldTypeName} implementations found"));
                menu.AddDisabledItem(new GUIContent("Create a non-abstract class implementing it to list it here"));
            }

            foreach (Type type in types)
            {
                Type captured = type;
                menu.AddItem(new GUIContent(captured.Name), false, () =>
                {
                    dataProp.managedReferenceValue = Activator.CreateInstance(captured);
                    dataProp.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        private List<Type> GetConcreteTypes(SerializedProperty dataProp)
        {
            // Resolve T1 from the managed reference field typename
            string[] parts = dataProp.managedReferenceFieldTypename.Split(' ');
            string typeName = parts.Length > 1 ? parts[1] : parts[0];

            Type fieldType = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                fieldType = asm.GetType(typeName);
                if (fieldType != null) break;
            }

            if (fieldType == null) return new List<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract && !t.IsInterface && fieldType.IsAssignableFrom(t)
                    && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name)
                .ToList();
        }

        private string GetManagedReferenceFieldTypeName(SerializedProperty dataProp)
        {
            string[] parts = dataProp.managedReferenceFieldTypename.Split(' ');
            string typeName = parts.Length > 1 ? parts[1] : parts[0];
            int lastDot = typeName.LastIndexOf('.');
            return lastDot >= 0 ? typeName.Substring(lastDot + 1) : typeName;
        }

        private IEnumerable<SerializedProperty> GetDirectChildren(SerializedProperty parent)
        {
            SerializedProperty copy = parent.Copy();
            int depth = copy.depth;
            bool entered = copy.NextVisible(true);
            if (!entered) yield break;

            while (copy.depth > depth)
            {
                yield return copy.Copy();
                if (!copy.NextVisible(false)) break;
            }
        }
    }
}
