using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    [CustomPropertyDrawer(typeof(Scaffold.AnimatorData))]
    [CustomPropertyDrawer(typeof(Scaffold.AudioClipData))]
    [CustomPropertyDrawer(typeof(Scaffold.AudioSourceData))]
    [CustomPropertyDrawer(typeof(Scaffold.BooleanData))]
    [CustomPropertyDrawer(typeof(Scaffold.ButtonData))]
    [CustomPropertyDrawer(typeof(Scaffold.CharacterData))]
    [CustomPropertyDrawer(typeof(Scaffold.CollectionData))]
    [CustomPropertyDrawer(typeof(Scaffold.Collider2DData))]
    [CustomPropertyDrawer(typeof(Scaffold.ColliderData))]
    [CustomPropertyDrawer(typeof(Scaffold.ColorData))]
    [CustomPropertyDrawer(typeof(Scaffold.FloatData))]
    [CustomPropertyDrawer(typeof(Scaffold.GameObjectData))]
    [CustomPropertyDrawer(typeof(Scaffold.IntegerData))]
    [CustomPropertyDrawer(typeof(Scaffold.MaterialData))]
    [CustomPropertyDrawer(typeof(Scaffold.Matrix4x4Data))]
    [CustomPropertyDrawer(typeof(Scaffold.ObjectData))]
    [CustomPropertyDrawer(typeof(Scaffold.QuaternionData))]
    [CustomPropertyDrawer(typeof(Scaffold.Rigidbody2DData))]
    [CustomPropertyDrawer(typeof(Scaffold.RigidbodyData))]
    [CustomPropertyDrawer(typeof(Scaffold.SpriteData))]
    [CustomPropertyDrawer(typeof(Scaffold.StringData))]
    [CustomPropertyDrawer(typeof(Scaffold.StringDataMulti))]
    [CustomPropertyDrawer(typeof(Scaffold.TextureData))]
    [CustomPropertyDrawer(typeof(Scaffold.TransformData))]
    [CustomPropertyDrawer(typeof(Scaffold.Vector2Data))]
    [CustomPropertyDrawer(typeof(Scaffold.Vector3Data))]
    [CustomPropertyDrawer(typeof(Scaffold.Vector4Data))]
    public sealed class BlackboardCompatibilityVariableDataDrawer :
        PropertyDrawer
    {
        private static readonly GUIContent[] s_sourceLabels =
        {
            new GUIContent(
                "Direct",
                "Use a value stored directly on this action."),
            new GUIContent(
                "Variable",
                "Read the value from a compatible Blackboard variable."),
            new GUIContent(
                "Asset",
                "Read the value from a compatible ScriptableObject asset."),
        };

        private const float k_horizontalSpacing = 4f;
        private const float k_sourceWidth = 76f;
        private const float k_minimumSourceWidth = 56f;
        private const float k_minimumLabelWidth = 80f;
        private const float k_maximumLabelWidth = 160f;
        private const float k_minimumValueWidth = 100f;
        private const float k_labelPadding = 6f;
        private static BlackboardDefinition s_currentDefinition;

        public static IReadOnlyList<VariableDefinitionBase>
            GetCompatibleVariables(
                BlackboardDefinition definition,
                Type expectedValueType)
        {
            List<VariableDefinitionBase> compatible =
                new List<VariableDefinitionBase>();
            if (definition == null || expectedValueType == null)
            {
                return compatible;
            }

            for (int index = 0;
                 index < definition.Variables.Count;
                 index++)
            {
                VariableDefinitionBase variable =
                    definition.Variables[index];
                if (IsCompatible(variable, expectedValueType))
                {
                    compatible.Add(variable);
                }
            }

            return compatible;
        }

        internal static IDisposable UseDefinition(
            BlackboardDefinition definition)
        {
            return new DefinitionScope(definition);
        }

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty source =
                property.FindPropertyRelative("source");
            SerializedProperty variable = FindChild(property, "Ref");
            SerializedProperty scriptableObject =
                FindChild(property, "SO");
            SerializedProperty directValue =
                FindChild(property, "Val");
            if (source == null ||
                variable == null ||
                scriptableObject == null ||
                directValue == null)
            {
                DrawFallback(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            Rect content = EditorGUI.IndentedRect(position);
            Scaffold.VariableDataSource resolvedSource =
                ResolveSource(source);
            SerializedProperty selectedValue = resolvedSource ==
                Scaffold.VariableDataSource.ScriptableObject
                ? scriptableObject
                : resolvedSource ==
                    Scaffold.VariableDataSource.BlackboardVariable
                    ? null
                    : directValue;
            bool stackValue = selectedValue != null &&
                EditorGUI.GetPropertyHeight(
                    selectedValue,
                    GUIContent.none,
                    true) >
                EditorGUIUtility.singleLineHeight;
            float sourceWidth = Mathf.Clamp(
                    content.width -
                    k_minimumLabelWidth -
                    k_minimumValueWidth -
                    (k_horizontalSpacing * 2f),
                k_minimumSourceWidth,
                k_sourceWidth);
            float maximumLabelWidth = Mathf.Max(
                0f,
                content.width -
                sourceWidth -
                k_minimumValueWidth -
                (k_horizontalSpacing * 2f));
            float preferredLabelWidth = Mathf.Clamp(
                EditorStyles.label.CalcSize(label).x + k_labelPadding,
                k_minimumLabelWidth,
                k_maximumLabelWidth);
            float labelWidth = Mathf.Min(
                preferredLabelWidth,
                maximumLabelWidth);
            if (stackValue)
            {
                labelWidth = Mathf.Min(
                    preferredLabelWidth,
                    content.width -
                    k_sourceWidth -
                    k_horizontalSpacing);
                sourceWidth = Mathf.Min(
                    k_sourceWidth,
                    content.width -
                    labelWidth -
                    k_horizontalSpacing);
            }

            Rect labelRect = new Rect(
                content.x,
                content.y,
                labelWidth,
                EditorGUIUtility.singleLineHeight);
            Rect sourceRect = new Rect(
                labelRect.xMax + k_horizontalSpacing,
                content.y,
                sourceWidth,
                EditorGUIUtility.singleLineHeight);
            if (stackValue)
            {
                sourceRect.x = content.xMax - sourceWidth;
            }

            Rect valueRect = new Rect(
                stackValue
                    ? content.x
                    : sourceRect.xMax + k_horizontalSpacing,
                stackValue
                    ? content.y +
                        EditorGUIUtility.singleLineHeight +
                        EditorGUIUtility.standardVerticalSpacing
                    : content.y,
                stackValue
                    ? content.width
                    : Mathf.Max(
                        0f,
                        content.xMax -
                        sourceRect.xMax -
                        k_horizontalSpacing),
                stackValue
                    ? Mathf.Max(
                        0f,
                        position.height -
                        EditorGUIUtility.singleLineHeight -
                        EditorGUIUtility.standardVerticalSpacing)
                    : position.height);

            EditorGUI.LabelField(labelRect, label);
            int selectedSource = ToPopupIndex(resolvedSource);
            int nextSource = EditorGUI.Popup(
                sourceRect,
                selectedSource,
                s_sourceLabels);
            if (nextSource != selectedSource)
            {
                resolvedSource = FromPopupIndex(nextSource);
                source.enumValueIndex = (int)resolvedSource;
            }

            DrawValue(
                valueRect,
                property,
                variable,
                scriptableObject,
                directValue,
                resolvedSource);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty source =
                property.FindPropertyRelative("source");
            if (source == null)
            {
                return EditorGUI.GetPropertyHeight(
                    property,
                    label,
                    true);
            }

            Scaffold.VariableDataSource resolvedSource =
                ResolveSource(source);
            SerializedProperty value = resolvedSource ==
                Scaffold.VariableDataSource.ScriptableObject
                ? FindChild(property, "SO")
                : resolvedSource ==
                    Scaffold.VariableDataSource.BlackboardVariable
                    ? null
                    : FindChild(property, "Val");
            if (value == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float valueHeight = EditorGUI.GetPropertyHeight(
                value,
                GUIContent.none,
                true);
            return valueHeight > EditorGUIUtility.singleLineHeight
                ? EditorGUIUtility.singleLineHeight +
                    EditorGUIUtility.standardVerticalSpacing +
                    valueHeight
                : EditorGUIUtility.singleLineHeight;
        }

        private static void DrawValue(
            Rect position,
            SerializedProperty property,
            SerializedProperty variable,
            SerializedProperty scriptableObject,
            SerializedProperty directValue,
            Scaffold.VariableDataSource source)
        {
            if (source ==
                Scaffold.VariableDataSource.BlackboardVariable)
            {
                DrawVariablePopup(
                    position,
                    variable,
                    GetExpectedValueType(property.type));
                return;
            }

            SerializedProperty selected = source ==
                Scaffold.VariableDataSource.ScriptableObject
                ? scriptableObject
                : directValue;
            EditorGUI.PropertyField(
                position,
                selected,
                GUIContent.none,
                true);
        }

        internal static void DrawVariablePopup(
            Rect position,
            SerializedProperty variable,
            Type expectedValueType)
        {
            DrawVariablePopup(
                position,
                variable,
                new[] { expectedValueType });
        }

        internal static void DrawVariablePopup(
            Rect position,
            SerializedProperty variable,
            IReadOnlyList<Type> expectedValueTypes)
        {
            SerializedProperty key =
                variable.FindPropertyRelative("key");
            SerializedProperty scope =
                variable.FindPropertyRelative("scope");
            SerializedProperty definitionId = variable
                .FindPropertyRelative("definitionId")
                ?.FindPropertyRelative("value");
            if (key == null ||
                scope == null ||
                definitionId == null)
            {
                EditorGUI.LabelField(
                    position,
                    "Invalid variable reference");
                return;
            }

            IReadOnlyList<VariableDefinitionBase> compatible =
                GetCompatibleVariables(
                    s_currentDefinition,
                    expectedValueTypes);
            List<string> choices = new List<string> { "None" };
            int selected = FindSelection(
                compatible,
                definitionId.stringValue,
                key.stringValue);
            for (int index = 0; index < compatible.Count; index++)
            {
                choices.Add(compatible[index].Key);
            }

            if (selected == 0 &&
                (!string.IsNullOrWhiteSpace(definitionId.stringValue) ||
                 !string.IsNullOrWhiteSpace(key.stringValue)))
            {
                choices.Add(
                    $"Missing: {key.stringValue}");
                selected = choices.Count - 1;
            }

            int next = EditorGUI.Popup(
                position,
                selected,
                choices.ToArray());
            if (next == selected)
            {
                MigrateLegacyReference(
                    compatible,
                    definitionId,
                    key,
                    scope);
                return;
            }

            if (next <= 0 || next > compatible.Count)
            {
                ClearReference(definitionId, key);
                return;
            }

            SetReference(
                compatible[next - 1],
                definitionId,
                key,
                scope);
        }

        internal static BlackboardDefinition CurrentDefinition =>
            s_currentDefinition;

        internal static IReadOnlyList<VariableDefinitionBase>
            GetCompatibleVariables(
                BlackboardDefinition definition,
                IReadOnlyList<Type> expectedValueTypes)
        {
            List<VariableDefinitionBase> compatible =
                new List<VariableDefinitionBase>();
            if (definition == null || expectedValueTypes == null)
            {
                return compatible;
            }

            for (int variableIndex = 0;
                 variableIndex < definition.Variables.Count;
                 variableIndex++)
            {
                VariableDefinitionBase variable =
                    definition.Variables[variableIndex];
                for (int typeIndex = 0;
                     typeIndex < expectedValueTypes.Count;
                     typeIndex++)
                {
                    if (IsCompatible(
                            variable,
                            expectedValueTypes[typeIndex]))
                    {
                        compatible.Add(variable);
                        break;
                    }
                }
            }

            return compatible;
        }

        internal static bool IsCompatible(
            VariableDefinitionBase variable,
            Type expectedValueType)
        {
            if (variable == null)
            {
                return false;
            }

            if (variable.ValueType == expectedValueType)
            {
                return true;
            }

            if (!(variable is UnityObjectVariableDefinition objectVariable) ||
                !typeof(UnityEngine.Object)
                    .IsAssignableFrom(expectedValueType))
            {
                return false;
            }

            UnityEngine.Object initialValue =
                objectVariable.InitialValue;
            return initialValue == null ||
                expectedValueType.IsInstanceOfType(initialValue);
        }

        private static int FindSelection(
            IReadOnlyList<VariableDefinitionBase> compatible,
            string definitionId,
            string key)
        {
            for (int index = 0; index < compatible.Count; index++)
            {
                VariableDefinitionBase candidate = compatible[index];
                if ((!string.IsNullOrWhiteSpace(definitionId) &&
                     candidate.DefinitionId.Value == definitionId) ||
                    (string.IsNullOrWhiteSpace(definitionId) &&
                     string.Equals(
                         candidate.Key,
                         key,
                         StringComparison.OrdinalIgnoreCase)))
                {
                    return index + 1;
                }
            }

            return 0;
        }

        private static void MigrateLegacyReference(
            IReadOnlyList<VariableDefinitionBase> compatible,
            SerializedProperty definitionId,
            SerializedProperty key,
            SerializedProperty scope)
        {
            if (!string.IsNullOrWhiteSpace(definitionId.stringValue))
            {
                return;
            }

            int selected = FindSelection(
                compatible,
                string.Empty,
                key.stringValue);
            if (selected > 0)
            {
                SetReference(
                    compatible[selected - 1],
                    definitionId,
                    key,
                    scope);
            }
        }

        private static void SetReference(
            VariableDefinitionBase selected,
            SerializedProperty definitionId,
            SerializedProperty key,
            SerializedProperty scope)
        {
            definitionId.stringValue =
                selected.DefinitionId.Value;
            key.stringValue = selected.Key;
            scope.enumValueIndex = (int)selected.Scope;
        }

        private static void ClearReference(
            SerializedProperty definitionId,
            SerializedProperty key)
        {
            definitionId.stringValue = string.Empty;
            key.stringValue = string.Empty;
        }

        private static Type GetExpectedValueType(
            string propertyType)
        {
            Type dataType = GetCompatibilityDataType(propertyType);
            if (dataType == null)
            {
                return null;
            }

            FieldInfo[] fields = dataType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            for (int index = 0; index < fields.Length; index++)
            {
                Type valueType = GetVariableValueType(
                    fields[index].FieldType);
                if (valueType != null)
                {
                    return valueType;
                }
            }

            return null;
        }

        internal static Type GetVariableValueType(
            Type variableType)
        {
            for (Type current = variableType;
                 current != null;
                 current = current.BaseType)
            {
                if (current.IsGenericType &&
                    current.GetGenericTypeDefinition() ==
                    typeof(Scaffold.VariableBase<>))
                {
                    return current.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private static Type GetCompatibilityDataType(
            string propertyType)
        {
            Type[] types =
            {
                typeof(Scaffold.AnimatorData),
                typeof(Scaffold.AudioClipData),
                typeof(Scaffold.AudioSourceData),
                typeof(Scaffold.BooleanData),
                typeof(Scaffold.ButtonData),
                typeof(Scaffold.CharacterData),
                typeof(Scaffold.CollectionData),
                typeof(Scaffold.Collider2DData),
                typeof(Scaffold.ColliderData),
                typeof(Scaffold.ColorData),
                typeof(Scaffold.FloatData),
                typeof(Scaffold.GameObjectData),
                typeof(Scaffold.IntegerData),
                typeof(Scaffold.MaterialData),
                typeof(Scaffold.Matrix4x4Data),
                typeof(Scaffold.ObjectData),
                typeof(Scaffold.QuaternionData),
                typeof(Scaffold.Rigidbody2DData),
                typeof(Scaffold.RigidbodyData),
                typeof(Scaffold.SpriteData),
                typeof(Scaffold.StringData),
                typeof(Scaffold.StringDataMulti),
                typeof(Scaffold.TextureData),
                typeof(Scaffold.TransformData),
                typeof(Scaffold.Vector2Data),
                typeof(Scaffold.Vector3Data),
                typeof(Scaffold.Vector4Data),
            };
            for (int index = 0; index < types.Length; index++)
            {
                if (types[index].Name == propertyType)
                {
                    return types[index];
                }
            }

            return null;
        }

        private static SerializedProperty FindChild(
            SerializedProperty property,
            string suffix)
        {
            SerializedProperty current = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;
            while (current.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(current, end))
            {
                enterChildren = false;
                if (current.depth == property.depth + 1 &&
                    current.name.EndsWith(
                        suffix,
                        StringComparison.Ordinal))
                {
                    return current.Copy();
                }
            }

            return null;
        }

        private static Scaffold.VariableDataSource ResolveSource(
            SerializedProperty source)
        {
            Scaffold.VariableDataSource value =
                (Scaffold.VariableDataSource)source.enumValueIndex;
            return value == Scaffold.VariableDataSource.Unspecified
                ? Scaffold.VariableDataSource.Direct
                : value;
        }

        private static int ToPopupIndex(
            Scaffold.VariableDataSource source)
        {
            switch (source)
            {
                case Scaffold.VariableDataSource.BlackboardVariable:
                    return 1;
                case Scaffold.VariableDataSource.ScriptableObject:
                    return 2;
                default:
                    return 0;
            }
        }

        private static Scaffold.VariableDataSource FromPopupIndex(
            int index)
        {
            switch (index)
            {
                case 1:
                    return Scaffold.VariableDataSource.BlackboardVariable;
                case 2:
                    return Scaffold.VariableDataSource.ScriptableObject;
                default:
                    return Scaffold.VariableDataSource.Direct;
            }
        }

        private static void DrawFallback(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.LabelField(
                position,
                label,
                new GUIContent(
                    $"Unsupported {property.type} reference"));
        }

        [CustomPropertyDrawer(
            typeof(Scaffold.VariablePropertyAttribute))]
        public sealed class VariablePropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(
                Rect position,
                SerializedProperty property,
                GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                Scaffold.VariablePropertyAttribute settings =
                    attribute as Scaffold.VariablePropertyAttribute;
                Type[] configuredTypes = settings?.VariableTypes;
                IReadOnlyList<Type> variableTypes =
                    configuredTypes == null ||
                    configuredTypes.Length == 0
                        ? Scaffold.AllVariableTypes
                            .AllScaffoldVarTypes
                        : configuredTypes;
                List<Type> valueTypes =
                    GetValueTypes(variableTypes);
                IReadOnlyList<VariableDefinitionBase> compatible =
                    GetCompatibleVariables(
                        s_currentDefinition,
                        valueTypes);
                DrawPopup(
                    position,
                    property,
                    label,
                    compatible,
                    variableTypes);
                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(
                SerializedProperty property,
                GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            private static void DrawPopup(
                Rect position,
                SerializedProperty property,
                GUIContent label,
                IReadOnlyList<VariableDefinitionBase> compatible,
                IReadOnlyList<Type> variableTypes)
            {
                SerializedProperty definitionId = property
                    .FindPropertyRelative("definitionId")
                    ?.FindPropertyRelative("value");
                SerializedProperty key =
                    property.FindPropertyRelative("key");
                int selected = FindSelection(
                    compatible,
                    definitionId?.stringValue,
                    key?.stringValue);
                List<string> choices =
                    BuildChoices(
                        compatible,
                        definitionId,
                        key,
                        ref selected);
                int next = EditorGUI.Popup(
                    position,
                    label.text,
                    selected,
                    choices.ToArray());
                if (next != selected)
                {
                    ApplySelection(
                        property,
                        compatible,
                        variableTypes,
                        next);
                }
            }

            private static List<Type> GetValueTypes(
                IReadOnlyList<Type> variableTypes)
            {
                List<Type> valueTypes = new List<Type>();
                for (int index = 0;
                     index < variableTypes.Count;
                     index++)
                {
                    Type valueType = GetVariableValueType(
                        variableTypes[index]);
                    if (valueType != null &&
                        !valueTypes.Contains(valueType))
                    {
                        valueTypes.Add(valueType);
                    }
                }

                return valueTypes;
            }

            private static List<string> BuildChoices(
                IReadOnlyList<VariableDefinitionBase> compatible,
                SerializedProperty definitionId,
                SerializedProperty key,
                ref int selected)
            {
                List<string> choices =
                    new List<string> { "None" };
                for (int index = 0;
                     index < compatible.Count;
                     index++)
                {
                    choices.Add(compatible[index].Key);
                }

                bool hasReference = definitionId != null &&
                    !string.IsNullOrWhiteSpace(
                        definitionId.stringValue);
                hasReference |= key != null &&
                    !string.IsNullOrWhiteSpace(
                        key.stringValue);
                if (selected == 0 && hasReference)
                {
                    choices.Add(
                        $"Missing: {key?.stringValue}");
                    selected = choices.Count - 1;
                }

                return choices;
            }

            private static void ApplySelection(
                SerializedProperty property,
                IReadOnlyList<VariableDefinitionBase> compatible,
                IReadOnlyList<Type> variableTypes,
                int selected)
            {
                if (selected <= 0 ||
                    selected > compatible.Count)
                {
                    ClearPropertyReference(property);
                    return;
                }

                VariableDefinitionBase definition =
                    compatible[selected - 1];
                EnsureManagedReference(
                    property,
                    definition,
                    variableTypes);
                SerializedProperty definitionId = property
                    .FindPropertyRelative("definitionId")
                    ?.FindPropertyRelative("value");
                SerializedProperty key =
                    property.FindPropertyRelative("key");
                SerializedProperty scope =
                    property.FindPropertyRelative("scope");
                if (definitionId == null ||
                    key == null ||
                    scope == null)
                {
                    Debug.LogError(
                        $"Compatibility variable property " +
                        $"'{property.propertyPath}' could not " +
                        "store the selected Blackboard reference.");
                    return;
                }

                SetReference(
                    definition,
                    definitionId,
                    key,
                    scope);
            }

            private static void EnsureManagedReference(
                SerializedProperty property,
                VariableDefinitionBase definition,
                IReadOnlyList<Type> variableTypes)
            {
                if (property.propertyType !=
                    SerializedPropertyType.ManagedReference)
                {
                    return;
                }

                Type variableType = FindVariableType(
                    definition,
                    variableTypes);
                if (variableType == null)
                {
                    throw new InvalidOperationException(
                        $"No compatibility variable type " +
                        $"accepts '{definition.ValueType.Name}'.");
                }

                if (property.managedReferenceValue == null ||
                    property.managedReferenceValue.GetType() !=
                    variableType)
                {
                    property.managedReferenceValue =
                        Activator.CreateInstance(variableType);
                }
            }

            private static Type FindVariableType(
                VariableDefinitionBase definition,
                IReadOnlyList<Type> variableTypes)
            {
                for (int index = 0;
                     index < variableTypes.Count;
                     index++)
                {
                    Type valueType = GetVariableValueType(
                        variableTypes[index]);
                    if (IsCompatible(definition, valueType))
                    {
                        return variableTypes[index];
                    }
                }

                return null;
            }

            private static void ClearPropertyReference(
                SerializedProperty property)
            {
                if (property.propertyType ==
                    SerializedPropertyType.ManagedReference)
                {
                    property.managedReferenceValue = null;
                    return;
                }

                SerializedProperty definitionId = property
                    .FindPropertyRelative("definitionId")
                    ?.FindPropertyRelative("value");
                SerializedProperty key =
                    property.FindPropertyRelative("key");
                if (definitionId != null)
                {
                    definitionId.stringValue = string.Empty;
                }

                if (key != null)
                {
                    key.stringValue = string.Empty;
                }
            }
        }

        [CustomPropertyDrawer(
            typeof(Scaffold.AnyVariableAndDataPair))]
        public sealed class AnyVariableAndDataPairDrawer :
            PropertyDrawer
        {
            public override void OnGUI(
                Rect position,
                SerializedProperty property,
                GUIContent label)
            {
                EditorGUI.BeginProperty(
                    position,
                    label,
                    property);
                SerializedProperty variable =
                    property.FindPropertyRelative("variable");
                SerializedProperty value =
                    GetSelectedValue(property);
                Rect variableRect = new Rect(
                    position.x,
                    position.y,
                    position.width,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(
                    variableRect,
                    variable,
                    label);
                if (value != null)
                {
                    float y = variableRect.yMax +
                        EditorGUIUtility.standardVerticalSpacing;
                    Rect valueRect = new Rect(
                        position.x,
                        y,
                        position.width,
                        EditorGUI.GetPropertyHeight(
                            value,
                            true));
                    EditorGUI.PropertyField(
                        valueRect,
                        value,
                        new GUIContent("Value"),
                        true);
                }

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(
                SerializedProperty property,
                GUIContent label)
            {
                SerializedProperty value =
                    GetSelectedValue(property);
                if (value == null)
                {
                    return EditorGUIUtility.singleLineHeight;
                }

                return EditorGUIUtility.singleLineHeight +
                    EditorGUIUtility.standardVerticalSpacing +
                    EditorGUI.GetPropertyHeight(value, true);
            }

            private static SerializedProperty GetSelectedValue(
                SerializedProperty property)
            {
                SerializedProperty variable =
                    property.FindPropertyRelative("variable");
                if (!(variable?.managedReferenceValue is
                    Scaffold.Variable selected))
                {
                    return null;
                }

                if (!Scaffold.AnyVariableAndDataPair
                    .s_typeActionLookup
                    .TryGetValue(
                        selected.GetType(),
                        out Scaffold.AnyVariableAndDataPair
                            .TypeActions actions))
                {
                    return null;
                }

                return property
                    .FindPropertyRelative("data")
                    ?.FindPropertyRelative(
                        actions.DataPropName);
            }
        }

        private sealed class DefinitionScope : IDisposable
        {
            public DefinitionScope(BlackboardDefinition definition)
            {
                previous = s_currentDefinition;
                s_currentDefinition = definition;
            }

            private readonly BlackboardDefinition previous;

            public void Dispose()
            {
                s_currentDefinition = previous;
            }
        }
    }
}
