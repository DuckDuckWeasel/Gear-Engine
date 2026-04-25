using System;
using System.Collections.Generic;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables.Editor
{
    /// <summary>Edit-time address resolution and validation. Mirrors the former AddressableScriptableObjectPublisherSORebaker.</summary>
    public static class AddressableBakeUtility
    {
        public static (string key, Type type)? ResolveSingle(AssetReference assetReference)
        {
            if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
            {
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(assetReference.AssetGUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (main == null)
            {
                return null;
            }

            string loadKey = ResolveAddressablesLoadKey(assetReference);
            if (string.IsNullOrEmpty(loadKey))
            {
                return null;
            }

            return (loadKey, main.GetType());
        }

        public static IPublisherRegistrar BakeAddressableSingle(AddressableSingleSource source)
        {
            if (source == null)
            {
                return null;
            }

            AssetReference ar = source.AssetReference;
            if (ar == null || string.IsNullOrEmpty(ar.AssetGUID))
            {
                return null;
            }

            (string key, Type type)? r = ResolveSingle(ar);
            if (r == null)
            {
                return null;
            }

            var (loadKey, assetType) = r.Value;
            Type registrarOpen = typeof(AddressableSinglePublisherRegistrar<>);
            Type closed = registrarOpen.MakeGenericType(assetType);
            return (IPublisherRegistrar)Activator.CreateInstance(closed, loadKey);
        }

        public static IPublisherRegistrar BakeAddressableLabel(AddressableLabelSource source)
        {
            if (source == null)
            {
                return null;
            }

            AssetLabelReference label = source.Label;
            if (label == null || string.IsNullOrEmpty(label.labelString))
            {
                return null;
            }

            Type t = ResolveLabelBakeType(label.labelString, source.TypeAqn);
            if (t == null)
            {
                return null;
            }

            ValidateLabelEntries(label.labelString, t);
            Type closed = typeof(AddressableLabelPublisherRegistrar<>).MakeGenericType(t);
            return (IPublisherRegistrar)Activator.CreateInstance(closed, label.labelString);
        }

        /// <summary>Returns the Type to bake the label registrar against. If the user picked a TypeAqn it wins (when valid); otherwise we infer the single common type from the label's entries.</summary>
        public static Type ResolveLabelBakeType(string labelString, string typeAqn)
        {
            if (!string.IsNullOrEmpty(typeAqn))
            {
                Type t = Type.GetType(typeAqn, throwOnError: false);
                if (t != null && typeof(UnityEngine.Object).IsAssignableFrom(t))
                {
                    return t;
                }
            }

            IReadOnlyList<Type> types = CollectLabelEntryTypes(labelString);
            if (types.Count == 1)
            {
                return types[0];
            }

            return null;
        }

        /// <summary>Distinct concrete asset types tagged with <paramref name="labelString"/> across all Addressable groups.</summary>
        public static IReadOnlyList<Type> CollectLabelEntryTypes(string labelString)
        {
            var seen = new HashSet<Type>();
            var ordered = new List<Type>();
            if (string.IsNullOrEmpty(labelString))
            {
                return ordered;
            }

            foreach (string groupGuid in AssetDatabase.FindAssets("t:AddressableAssetGroup"))
            {
                string path = AssetDatabase.GUIDToAssetPath(groupGuid);
                UnityEngine.Object groupObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (groupObj == null)
                {
                    continue;
                }

                var groupSo = new SerializedObject(groupObj);
                SerializedProperty entries = groupSo.FindProperty("m_SerializeEntries");
                if (entries == null || !entries.isArray)
                {
                    continue;
                }

                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty el = entries.GetArrayElementAtIndex(i);
                    if (!EntryHasLabel(el, labelString))
                    {
                        continue;
                    }

                    string guid = GetEntryGuid(el);
                    if (string.IsNullOrEmpty(guid))
                    {
                        continue;
                    }

                    string ap = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(ap))
                    {
                        continue;
                    }

                    UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(ap);
                    if (main == null)
                    {
                        continue;
                    }

                    Type tt = main.GetType();
                    if (seen.Add(tt))
                    {
                        ordered.Add(tt);
                    }
                }
            }

            return ordered;
        }

        public static void ValidateLabelEntries(string label, Type expectedType)
        {
            if (string.IsNullOrEmpty(label) || expectedType == null)
            {
                return;
            }

            foreach (string groupGuid in AssetDatabase.FindAssets("t:AddressableAssetGroup"))
            {
                string path = AssetDatabase.GUIDToAssetPath(groupGuid);
                UnityEngine.Object groupObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (groupObj == null)
                {
                    continue;
                }

                var groupSo = new SerializedObject(groupObj);
                SerializedProperty entries = groupSo.FindProperty("m_SerializeEntries");
                if (entries == null || !entries.isArray)
                {
                    continue;
                }

                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty el = entries.GetArrayElementAtIndex(i);
                    if (!EntryHasLabel(el, label))
                    {
                        continue;
                    }

                    string guid = GetEntryGuid(el);
                    if (string.IsNullOrEmpty(guid))
                    {
                        continue;
                    }

                    string ap = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(ap))
                    {
                        continue;
                    }

                    UnityEngine.Object a = AssetDatabase.LoadMainAssetAtPath(ap);
                    if (a == null)
                    {
                        continue;
                    }

                    if (!expectedType.IsAssignableFrom(a.GetType()))
                    {
                        Debug.LogWarning(
                            $"[AddressableLabelSource] Addressables entry for '{ap}' (type {a.GetType().Name}) is not assignable to expected {expectedType.Name} for label '{label}'.",
                            groupObj);
                    }
                }
            }
        }

        private static string GetEntryGuid(SerializedProperty el)
        {
            SerializedProperty g = el?.FindPropertyRelative("m_GUID");
            return g != null ? g.stringValue : null;
        }

        private static bool EntryHasLabel(SerializedProperty entry, string label)
        {
            SerializedProperty labels = entry.FindPropertyRelative("m_SerializedLabels");
            if (labels == null || !labels.isArray)
            {
                return false;
            }

            for (int k = 0; k < labels.arraySize; k++)
            {
                SerializedProperty p = labels.GetArrayElementAtIndex(k);
                string s = p?.stringValue;
                if (string.Equals(s, label, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveAddressablesLoadKey(AssetReference assetReference)
        {
            string fromGroups = TryFindAddressInAddressableGroups(assetReference.AssetGUID);
            if (!string.IsNullOrEmpty(fromGroups))
            {
                return fromGroups;
            }

            object runtimeKey = assetReference.RuntimeKey;
            return runtimeKey?.ToString();
        }

        private static string TryFindAddressInAddressableGroups(string assetGuid)
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                return null;
            }

            foreach (string groupGuid in AssetDatabase.FindAssets("t:AddressableAssetGroup"))
            {
                string path = AssetDatabase.GUIDToAssetPath(groupGuid);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj == null)
                {
                    continue;
                }

                var groupSo = new SerializedObject(obj);
                SerializedProperty serEntries = groupSo.FindProperty("m_SerializeEntries");
                if (serEntries == null || !serEntries.isArray)
                {
                    continue;
                }

                for (int i = 0; i < serEntries.arraySize; i++)
                {
                    SerializedProperty el = serEntries.GetArrayElementAtIndex(i);
                    SerializedProperty g = el.FindPropertyRelative("m_GUID");
                    SerializedProperty addr = el.FindPropertyRelative("m_Address");
                    if (g != null && addr != null &&
                        string.Equals(g.stringValue, assetGuid, StringComparison.Ordinal) &&
                        !string.IsNullOrEmpty(addr.stringValue))
                    {
                        return addr.stringValue;
                    }
                }
            }

            return null;
        }
    }
}
