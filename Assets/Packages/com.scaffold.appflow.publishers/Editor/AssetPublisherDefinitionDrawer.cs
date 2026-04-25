using System;
using System.Collections.Generic;
using System.Reflection;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Editor
{
    /// <summary>
    /// List row: status light (same Unity <c>lightMeter/*</c> icons as
    /// <c>Scaffold.LiveOps.Authoring.Editor.Window.LiveOpsConfigStatusLights</c>) on the left of the
    /// content, then type + source fields; fields use no left labels; single horizontal row when all fit.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetPublisherDefinition))]
    public sealed class AssetPublisherDefinitionDrawer : PropertyDrawer
    {
        // Same icon asset paths as LiveOpsConfigStatusLights
        private const string IconInSync = "lightMeter/greenLight";
        private const string IconDrift = "lightMeter/orangeLight";
        private const string IconMissing = "lightMeter/redLight";
        private const string IconNeutral = "lightMeter/lightRim";

        private const float LightSize = 14f;
        private const float LightRightMargin = 6f;
        private const float ControlGap = 6f;
        private const float FieldColumnGap = 4f;
        private const float PopupWidthMin = 100f;
        private const float PopupWidthMax = 240f;
        private const float PopupWidthFraction = 0.3f;

        /// <summary>Do not use <see cref="EditorGUIUtility.labelWidth"/> for the list row: it is often 40–50% of the inspector, while "Element 0" only needs a few pixels — the rest was dead space.</summary>
        private const float MaxPrefixLabelColumnPx = 160f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float vspace = EditorGUIUtility.standardVerticalSpacing;
            int id = GUIUtility.GetControlID(FocusType.Passive, position);
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float labelW = MeasurePrefixLabelWidth(label);
            float rowY = position.y;
            float contentLeft = position.x + labelW;
            float contentW = position.width - labelW;

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sourceProp = property.FindPropertyRelative("source");
            SerializedProperty bakedProp = property.FindPropertyRelative("bakedRegistrar");

            Type[] types = GetSourceTypes();
            var names = new string[types.Length + 1];
            names[0] = "None";
            for (int i = 0; i < types.Length; i++)
            {
                names[i + 1] = types[i].Name;
            }

            int current = 0;
            if (sourceProp.managedReferenceValue != null)
            {
                Type vt = sourceProp.managedReferenceValue.GetType();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == vt)
                    {
                        current = i + 1;
                        break;
                    }
                }
            }

            if (labelW > 0f)
            {
                EditorGUI.PrefixLabel(new Rect(position.x, rowY, labelW, line), id, label);
            }

            var lightSlot = new Rect(contentLeft, rowY, LightSize, line);
            float xAfterLight = contentLeft + LightSize + LightRightMargin;
            float popW = Mathf.Clamp(contentW * PopupWidthFraction, PopupWidthMin, PopupWidthMax);
            popW = Mathf.Min(
                popW,
                Mathf.Max(80f, contentW - LightSize - LightRightMargin - ControlGap * 2));
            var popupR = new Rect(xAfterLight, rowY, popW, line);
            float fieldsLeft = popupR.xMax + ControlGap;
            float fieldsW = (contentLeft + contentW) - fieldsLeft;
            if (fieldsW < 1f)
            {
                fieldsW = 1f;
            }

            var firstRowFields = new Rect(fieldsLeft, rowY, fieldsW, line);

            EditorGUI.BeginChangeCheck();
            int next = EditorGUI.Popup(popupR, current, names);
            if (next != current)
            {
                if (next == 0)
                {
                    sourceProp.managedReferenceValue = null;
                }
                else
                {
                    sourceProp.managedReferenceValue = Activator.CreateInstance(types[next - 1]);
                }
            }

            if (sourceProp.managedReferenceValue != null)
            {
                using (new LabelWidthScope(0f))
                {
                    DrawSourceFields(
                        position,
                        labelW,
                        line,
                        vspace,
                        firstRowFields,
                        sourceProp);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                sourceProp = property.FindPropertyRelative("source");
                bakedProp = property.FindPropertyRelative("bakedRegistrar");
                RunBakeFromProps(sourceProp, bakedProp, property);
                sourceProp = property.FindPropertyRelative("source");
                bakedProp = property.FindPropertyRelative("bakedRegistrar");
            }

            DrawStatusLightIcon(lightSlot, sourceProp, bakedProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        private readonly struct LabelWidthScope : IDisposable
        {
            private readonly float _previous;

            public LabelWidthScope(float width)
            {
                _previous = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = width;
            }

            public void Dispose() => EditorGUIUtility.labelWidth = _previous;
        }

        private static void DrawStatusLightIcon(Rect r, SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            string iconId = StatusIconId(sourceProp, bakedProp);
            GUIContent g = EditorGUIUtility.IconContent(iconId);
            if (g == null || g.image == null)
            {
                return;
            }

            var textureRect = new Rect(
                r.x,
                r.y + (r.height - LightSize) * 0.5f,
                LightSize,
                LightSize);
            GUI.DrawTexture(textureRect, g.image, ScaleMode.ScaleToFit);
            GUI.Label(r, new GUIContent(string.Empty, StatusTooltip(sourceProp, bakedProp)));
        }

        private static string StatusIconId(SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            if (sourceProp == null)
            {
                return IconNeutral;
            }

            if (sourceProp.managedReferenceValue is IAssetPublisherSource s)
            {
                if (s.IsConfigured)
                {
                    return bakedProp.managedReferenceValue != null ? IconInSync : IconDrift;
                }

                return IconMissing;
            }

            if (sourceProp.managedReferenceValue == null)
            {
                return IconNeutral;
            }

            return IconMissing;
        }

        private static string StatusTooltip(SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            if (sourceProp == null)
            {
                return string.Empty;
            }

            if (sourceProp.managedReferenceValue is IAssetPublisherSource s)
            {
                if (s.IsConfigured)
                {
                    return bakedProp.managedReferenceValue != null ? "Baked" : "Source OK — not baked";
                }

                return "Source incomplete (cannot bake)";
            }

            if (sourceProp.managedReferenceValue == null)
            {
                return "No source";
            }

            return "Rebake needed";
        }

        private static void DrawSourceFields(
            Rect position,
            float labelW,
            float line,
            float vspace,
            Rect firstRowFields,
            SerializedProperty sourceRoot)
        {
            if (sourceRoot == null || sourceRoot.managedReferenceValue == null)
            {
                return;
            }

            if (!TryCollectSourceChildren(sourceRoot, out List<SerializedProperty> children) || children.Count == 0)
            {
                return;
            }

            bool allSingleLine = true;
            float maxH = 0f;
            for (int i = 0; i < children.Count; i++)
            {
                var p = children[i].Copy();
                float h = EditorGUI.GetPropertyHeight(p, true);
                maxH = Mathf.Max(maxH, h);
                if (h > line + 0.1f)
                {
                    allSingleLine = false;
                }
            }

            if (allSingleLine)
            {
                int n = children.Count;
                float x = firstRowFields.x;
                for (int i = 0; i < n; i++)
                {
                    var p = children[i].Copy();
                    float h = EditorGUI.GetPropertyHeight(p, true);
                    float w = n == 1
                        ? firstRowFields.width
                        : (i == n - 1
                            ? (firstRowFields.xMax - x)
                            : (firstRowFields.width - (n - 1) * FieldColumnGap) / n);
                    var cell = new Rect(x, firstRowFields.y, w, h);
                    EditorGUI.PropertyField(cell, p, GUIContent.none, true);
                    x = cell.xMax + FieldColumnGap;
                }
            }
            else
            {
                // Type popup stays on row 1; wide fields go below, full content width, no labels.
                float y = firstRowFields.y + line + vspace;
                for (int i = 0; i < children.Count; i++)
                {
                    var p = children[i].Copy();
                    float h = EditorGUI.GetPropertyHeight(p, true);
                    var row = new Rect(position.x + labelW, y, position.width - labelW, h);
                    EditorGUI.PropertyField(row, p, GUIContent.none, true);
                    y += h + vspace;
                }
            }
        }

        private static bool TryCollectSourceChildren(SerializedProperty sourceRoot, out List<SerializedProperty> children)
        {
            children = new List<SerializedProperty>();
            if (sourceRoot == null)
            {
                return false;
            }

            SerializedProperty end = sourceRoot.GetEndProperty();
            SerializedProperty it = sourceRoot.Copy();
            if (!it.NextVisible(true))
            {
                return false;
            }

            do
            {
                if (it.name == "m_Script" || (it.name == "bakedAssetTypeAqn" && it.propertyType == SerializedPropertyType.String))
                {
                    continue;
                }

                children.Add(it.Copy());
            } while (it.NextVisible(false) && !SerializedProperty.EqualContents(it, end));

            return true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float vspace = EditorGUIUtility.standardVerticalSpacing;
            var sourceProp = property.FindPropertyRelative("source");
            if (sourceProp == null || sourceProp.managedReferenceValue == null)
            {
                return line;
            }

            if (!TryCollectSourceChildren(sourceProp, out List<SerializedProperty> children) || children.Count == 0)
            {
                return line;
            }

            bool allSingle = true;
            for (int i = 0; i < children.Count; i++)
            {
                var p = children[i].Copy();
                if (EditorGUI.GetPropertyHeight(p, true) > line + 0.1f)
                {
                    allSingle = false;
                    break;
                }
            }

            if (allSingle)
            {
                return line;
            }

            float h = line + vspace;
            for (int i = 0; i < children.Count; i++)
            {
                var p = children[i].Copy();
                h += EditorGUI.GetPropertyHeight(p, true) + vspace;
            }

            return h;
        }

        private static void RunBakeFromProps(
            SerializedProperty sourceProp,
            SerializedProperty bakedProp,
            SerializedProperty defProperty)
        {
            if (sourceProp.managedReferenceValue is IAssetPublisherSource s)
            {
                if (s.IsConfigured)
                {
                    bakedProp.managedReferenceValue = s.Bake();
                }
                else
                {
                    bakedProp.managedReferenceValue = null;
                }
            }
            else
            {
                bakedProp.managedReferenceValue = null;
            }

            defProperty.serializedObject.ApplyModifiedProperties();
        }

        private static Type[] GetSourceTypes()
        {
            TypeCache.TypeCollection fromCache = TypeCache.GetTypesDerivedFrom<IAssetPublisherSource>();
            var list = new List<Type>();
            foreach (Type t in fromCache)
            {
                if (t is not { IsAbstract: false, IsClass: true, IsGenericTypeDefinition: false } ||
                    t.GetCustomAttribute<SerializableAttribute>() == null)
                {
                    continue;
                }

                list.Add(t);
            }

            list.Sort(CompareTypeNames);
            return list.ToArray();
        }

        private static int CompareTypeNames(Type a, Type b)
        {
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }

        /// <summary>Text-sized prefix column. Whitespace-only labels (common for list rows) → 0 so controls start after the list foldout/handle, not a half-width column.</summary>
        private static float MeasurePrefixLabelWidth(GUIContent label)
        {
            if (label == null)
            {
                return 0f;
            }

            // Unity often passes " " to reserve layout while hiding the text — treat as no label.
            if (string.IsNullOrWhiteSpace(label.text) && (label.image == null))
            {
                return 0f;
            }

            float w = EditorStyles.label.CalcSize(label).x + 4f;
            return Mathf.Min(w, MaxPrefixLabelColumnPx);
        }
    }
}
