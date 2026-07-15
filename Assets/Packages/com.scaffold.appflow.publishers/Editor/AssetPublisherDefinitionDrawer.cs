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

        private static Type[] s_SourceTypes;
        private static string[] s_PopupTypeNames;
        private static int s_SourceTypeCacheRebuilds;

        /// <summary>Number of times the static source-type list was built. Exposed for tests.</summary>
        internal static int SourceTypeCacheRebuilds => s_SourceTypeCacheRebuilds;

        private static readonly Dictionary<int, (UnityEngine.Object target, string defPath)> s_PendingRebake = new();

        private static bool s_PendingRebakeRegistered;

        private static readonly GUIContent s_StatusLightTooltip = new();

        private static string s_LastStatusTooltip = string.Empty;

        /// <summary>Full serialized property paths for <c>source</c> child fields, keyed to avoid re-walking the tree. Values are <see cref="string"/>-only so they stay valid after layout.</summary>
        private static readonly Dictionary<string, List<string>> s_ChildPropertyPaths = new();

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            AssemblyReloadEvents.afterAssemblyReload += ClearSourceTypeCache;
        }

        private static void ClearSourceTypeCache()
        {
            s_SourceTypes = null;
            s_PopupTypeNames = null;
            s_ChildPropertyPaths.Clear();
        }

        /// <summary>Clears the static type cache. For tests; domain reload also clears via <see cref="AssemblyReloadEvents.afterAssemblyReload"/>.</summary>
        internal static void ResetSourceTypeCacheForTests()
        {
            s_SourceTypeCacheRebuilds = 0;
            ClearSourceTypeCache();
        }

        /// <summary>Populates the static type popup cache if needed. Exposed for tests.</summary>
        internal static void EnsureSourceTypeCacheForTests() => EnsureSourceTypeCache();

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

            EnsureSourceTypeCache();
            Type[] types = s_SourceTypes;
            string[] names = s_PopupTypeNames;
            if (sourceProp == null)
            {
                EditorGUI.indentLevel = oldIndent;
                EditorGUI.EndProperty();
                return;
            }

            int current = 0;
            object sourceValue = sourceProp.managedReferenceValue;
            if (sourceValue != null)
            {
                Type vt = sourceValue.GetType();
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

            int oldPopup = current;
            EditorGUI.BeginChangeCheck();
            int next = EditorGUI.Popup(popupR, current, names);
            bool typeChanged = false;
            if (EditorGUI.EndChangeCheck() && next != oldPopup)
            {
                s_ChildPropertyPaths.Remove(MakeChildPathsCacheKey(property, sourceProp));
                if (next == 0)
                {
                    sourceProp.managedReferenceValue = null;
                }
                else
                {
                    sourceProp.managedReferenceValue = Activator.CreateInstance(types[next - 1]);
                }

                typeChanged = true;
                property.serializedObject.ApplyModifiedProperties();
                sourceProp = property.FindPropertyRelative("source");
                bakedProp = property.FindPropertyRelative("bakedRegistrar");
            }
            if (typeChanged)
            {
                RunBakeFromProps(sourceProp, bakedProp, property);
                sourceProp = property.FindPropertyRelative("source");
                bakedProp = property.FindPropertyRelative("bakedRegistrar");
            }

            if (sourceProp.managedReferenceValue != null)
            {
                using (new LabelWidthScope(0f))
                {
                    EditorGUI.BeginChangeCheck();
                    DrawSourceFields(
                        position,
                        labelW,
                        line,
                        vspace,
                        firstRowFields,
                        sourceProp,
                        property);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        sourceProp = property.FindPropertyRelative("source");
                        bakedProp = property.FindPropertyRelative("bakedRegistrar");
                        ScheduleDeferredRebake(property.serializedObject, property.propertyPath);
                    }
                }
            }

            sourceValue = sourceProp.managedReferenceValue;
            DrawStatusLightIcon(lightSlot, sourceValue, sourceProp, bakedProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        private static void ScheduleDeferredRebake(SerializedObject serializedObject, string definitionPropertyPath)
        {
            if (serializedObject == null)
            {
                return;
            }

            int id = GetDefinitionRebakeKey(serializedObject.targetObject, definitionPropertyPath);
            s_PendingRebake[id] = (serializedObject.targetObject, definitionPropertyPath);
            if (s_PendingRebakeRegistered)
            {
                return;
            }

            s_PendingRebakeRegistered = true;
            EditorApplication.delayCall += RunPendingRebakes;
        }

        private static int GetDefinitionRebakeKey(UnityEngine.Object target, string definitionPropertyPath)
        {
            if (string.IsNullOrEmpty(definitionPropertyPath))
            {
                return 0;
            }

            int targetId = target != null ? target.GetEntityId().GetHashCode() : 0;
            return (targetId * 397) ^ (definitionPropertyPath.GetHashCode(StringComparison.Ordinal) * 397);
        }

        private static void RunPendingRebakes()
        {
            s_PendingRebakeRegistered = false;
            if (s_PendingRebake.Count == 0)
            {
                return;
            }

            var work = new List<(UnityEngine.Object target, string defPath)>(s_PendingRebake.Values);
            s_PendingRebake.Clear();
            DeferredRebakeWorkItemCount = work.Count;
            for (int i = 0; i < work.Count; i++)
            {
                (UnityEngine.Object target, string defPath) = work[i];
                if (target == null)
                {
                    continue;
                }

                var so = new SerializedObject(target);
                SerializedProperty def = so.FindProperty(defPath);
                if (def == null)
                {
                    continue;
                }

                SerializedProperty s = def.FindPropertyRelative("source");
                SerializedProperty b = def.FindPropertyRelative("bakedRegistrar");
                if (s == null || b == null)
                {
                    continue;
                }

                RunBakeFromProps(s, b, def);
            }
        }

        /// <summary>Work items processed in the last <see cref="RunPendingRebakes"/> call. Exposed for tests.</summary>
        internal static int DeferredRebakeWorkItemCount { get; private set; }

        /// <summary>Enqueues a deferred rebake (same path as an inner-field change). Exposed for tests.</summary>
        internal static void EnqueueDeferredRebakeForTests(SerializedObject serializedObject, string definitionPropertyPath) =>
            ScheduleDeferredRebake(serializedObject, definitionPropertyPath);

        /// <summary>Runs the pending-rebake queue without waiting for <c>EditorApplication.delayCall</c>. Exposed for tests.</summary>
        internal static void FlushPendingRebakesForTests() => RunPendingRebakes();

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

        private static void DrawStatusLightIcon(Rect r, object sourceValue, SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            string iconId = StatusIconId(sourceValue, sourceProp, bakedProp);
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
            string tip = StatusTooltip(sourceValue, sourceProp, bakedProp);
            if (s_LastStatusTooltip != tip)
            {
                s_StatusLightTooltip.text = string.Empty;
                s_StatusLightTooltip.tooltip = tip;
                s_LastStatusTooltip = tip;
            }

            GUI.Label(r, s_StatusLightTooltip);
        }

        private static string StatusIconId(object sourceValue, SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            if (sourceProp == null)
            {
                return IconNeutral;
            }

            if (sourceValue is IAssetPublisherSource s)
            {
                if (s.IsConfigured)
                {
                    return bakedProp.managedReferenceValue != null ? IconInSync : IconDrift;
                }

                return IconMissing;
            }

            if (sourceValue == null)
            {
                return IconNeutral;
            }

            return IconMissing;
        }

        private static string StatusTooltip(object sourceValue, SerializedProperty sourceProp, SerializedProperty bakedProp)
        {
            if (sourceProp == null)
            {
                return string.Empty;
            }

            if (sourceValue is IAssetPublisherSource s)
            {
                if (s.IsConfigured)
                {
                    return bakedProp.managedReferenceValue != null ? "Baked" : "Source OK — not baked";
                }

                return "Source incomplete (cannot bake)";
            }

            if (sourceValue == null)
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
            SerializedProperty sourceRoot,
            SerializedProperty definitionProperty)
        {
            if (sourceRoot == null || sourceRoot.managedReferenceValue == null)
            {
                return;
            }

            if (!TryGetChildPropertyPathsForSource(sourceRoot, definitionProperty, out List<string> pathList) || pathList.Count == 0)
            {
                return;
            }

            var children = new List<SerializedProperty>(pathList.Count);
            for (int i = 0; i < pathList.Count; i++)
            {
                string childPath = GetRelativePathUnderSource(sourceRoot, pathList[i]);
                SerializedProperty p = !string.IsNullOrEmpty(childPath)
                    ? sourceRoot.FindPropertyRelative(childPath)
                    : sourceRoot;
                if (p == null)
                {
                    s_ChildPropertyPaths.Remove(MakeChildPathsCacheKey(definitionProperty, sourceRoot));
                    return;
                }

                children.Add(p);
            }

            bool allSingleLine = true;
            for (int i = 0; i < children.Count; i++)
            {
                SerializedProperty p = children[i];
                float h = EditorGUI.GetPropertyHeight(p, true);
                if (h > line + 0.1f)
                {
                    allSingleLine = false;
                    break;
                }
            }

            if (allSingleLine)
            {
                int n = children.Count;
                float x = firstRowFields.x;
                for (int i = 0; i < n; i++)
                {
                    SerializedProperty p = children[i];
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
                    SerializedProperty p = children[i];
                    float h = EditorGUI.GetPropertyHeight(p, true);
                    var row = new Rect(position.x + labelW, y, position.width - labelW, h);
                    EditorGUI.PropertyField(row, p, GUIContent.none, true);
                    y += h + vspace;
                }
            }
        }

        private static string MakeChildPathsCacheKey(SerializedProperty definitionProperty, SerializedProperty sourceProp)
        {
            UnityEngine.Object t = definitionProperty.serializedObject.targetObject;
            int id = t != null ? t.GetEntityId().GetHashCode() : 0;
            return $"{id}:{definitionProperty.propertyPath}|{sourceProp.managedReferenceFullTypename ?? "null"}";
        }

        private static string GetRelativePathUnderSource(SerializedProperty sourceRoot, string childFullPath)
        {
            if (sourceRoot == null || string.IsNullOrEmpty(childFullPath))
            {
                return null;
            }

            if (string.Equals(childFullPath, sourceRoot.propertyPath, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            string root = sourceRoot.propertyPath;
            if (!childFullPath.StartsWith(root + ".", StringComparison.Ordinal))
            {
                return null;
            }

            return childFullPath.Length > root.Length + 1 ? childFullPath[(root.Length + 1)..] : null;
        }

        private static bool TryGetChildPropertyPathsForSource(
            SerializedProperty sourceRoot,
            SerializedProperty definitionProperty,
            out List<string> pathList)
        {
            string key = MakeChildPathsCacheKey(definitionProperty, sourceRoot);
            if (s_ChildPropertyPaths.TryGetValue(key, out pathList) && pathList != null)
            {
                return pathList.Count > 0;
            }

            if (!TryCollectSourceChildPropertyPaths(sourceRoot, out pathList) || pathList == null)
            {
                return false;
            }

            s_ChildPropertyPaths[key] = pathList;
            return pathList.Count > 0;
        }

        private static bool TryCollectSourceChildPropertyPaths(SerializedProperty sourceRoot, out List<string> paths)
        {
            paths = new List<string>();
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

                paths.Add(it.propertyPath);
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

            if (!TryGetChildPropertyPathsForSource(sourceProp, property, out List<string> pathList) || pathList.Count == 0)
            {
                return line;
            }

            var children = new List<SerializedProperty>(pathList.Count);
            for (int i = 0; i < pathList.Count; i++)
            {
                string rel = GetRelativePathUnderSource(sourceProp, pathList[i]);
                SerializedProperty p = !string.IsNullOrEmpty(rel) ? sourceProp.FindPropertyRelative(rel) : sourceProp;
                if (p == null)
                {
                    s_ChildPropertyPaths.Remove(MakeChildPathsCacheKey(property, sourceProp));
                    return line;
                }

                children.Add(p);
            }

            bool allSingle = true;
            for (int i = 0; i < children.Count; i++)
            {
                if (EditorGUI.GetPropertyHeight(children[i], true) > line + 0.1f)
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
                h += EditorGUI.GetPropertyHeight(children[i], true) + vspace;
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

        private static void EnsureSourceTypeCache()
        {
            if (s_SourceTypes != null && s_PopupTypeNames != null)
            {
                return;
            }

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
            s_SourceTypes = list.ToArray();
            s_PopupTypeNames = new string[s_SourceTypes.Length + 1];
            s_PopupTypeNames[0] = "None";
            for (int i = 0; i < s_SourceTypes.Length; i++)
            {
                s_PopupTypeNames[i + 1] = s_SourceTypes[i].Name;
            }

            s_SourceTypeCacheRebuilds++;
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
