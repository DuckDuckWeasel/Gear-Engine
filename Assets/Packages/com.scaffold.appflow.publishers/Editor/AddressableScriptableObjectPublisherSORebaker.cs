using System;
using System.Reflection;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Editor
{
    /// <summary>
    /// Edit-time bake: writes a closed-generic <see cref="AddressableScriptableObjectPublisherRegistrar{T}"/> into the SO's SerializeReference field.
    /// </summary>
    public static class AddressableScriptableObjectPublisherSORebaker
    {
        public static void Rebake(AddressableScriptableObjectPublisherSO so)
        {
            RebakeIfStale(so, force: true);
        }

        /// <summary>
        /// Rebuilds the baked registrar when the asset reference or addressable key no longer matches the existing bake.
        /// </summary>
        public static void RebakeIfStale(AddressableScriptableObjectPublisherSO so, bool force = false)
        {
            if (so == null)
            {
                throw new ArgumentNullException(nameof(so));
            }

            AssetReference assetReference = so.AssetReference;
            SerializedObject serializedObject = new SerializedObject(so);
            SerializedProperty bakedProp = serializedObject.FindProperty("bakedRegistrar");

            if (bakedProp == null)
            {
                Debug.LogError($"[PublisherRebaker] Missing field bakedRegistrar on '{so.name}'.");
                return;
            }

            if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
            {
                ClearBake(bakedProp);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(so);
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(assetReference.AssetGUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"[PublisherRebaker] Unknown GUID for '{so.name}': {assetReference.AssetGUID}");
                return;
            }

            UnityEngine.Object main = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (main == null)
            {
                ClearBake(bakedProp);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(so);
                return;
            }

            if (main is not ScriptableObject scriptableObject)
            {
                Debug.LogError(
                    $"[PublisherRebaker] '{so.name}' asset reference must point to a ScriptableObject (got {main.GetType().Name}).");
                return;
            }

            Type assetType = scriptableObject.GetType();
            string loadKey = ResolveAddressablesLoadKey(assetReference);
            if (string.IsNullOrEmpty(loadKey))
            {
                Debug.LogWarning(
                    $"[PublisherRebaker] '{so.name}' has no Addressables load key; ensure the asset is Addressable. Clearing bake.");
                ClearBake(bakedProp);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(so);
                return;
            }

            if (!force && IsBakeCurrent(bakedProp, assetType, loadKey))
            {
                return;
            }

            Type registrarOpen = typeof(AddressableScriptableObjectPublisherRegistrar<>);
            Type registrarClosed;
            try
            {
                registrarClosed = registrarOpen.MakeGenericType(assetType);
            }
            catch (ArgumentException ex)
            {
                Debug.LogError($"[PublisherRebaker] Cannot build registrar for '{assetType.Name}' on '{so.name}': {ex.Message}");
                return;
            }

            object registrarInstance = Activator.CreateInstance(registrarClosed);
            bakedProp.managedReferenceValue = registrarInstance;
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            SerializedProperty bakedAfter = serializedObject.FindProperty("bakedRegistrar");
            SerializedProperty keyProp = bakedAfter.FindPropertyRelative("addressableKey");
            if (keyProp == null)
            {
                Debug.LogError($"[PublisherRebaker] Expected field 'addressableKey' on registrar for '{so.name}'.");
                return;
            }

            keyProp.stringValue = loadKey;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(so);
        }

        /// <summary>
        /// <see cref="Scaffold.Addressables.Contracts.IAddressablesAssetClient.LoadAssetAsync{T}"/> expects the catalog address string.
        /// <see cref="AssetReference.RuntimeKey"/> may be a GUID; resolve the entry address from serialized Addressable groups when possible.
        /// </summary>
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

                SerializedObject groupSo = new SerializedObject(obj);
                SerializedProperty entries = groupSo.FindProperty("m_SerializeEntries");
                if (entries == null || !entries.isArray)
                {
                    continue;
                }

                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty el = entries.GetArrayElementAtIndex(i);
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

        private static void ClearBake(SerializedProperty bakedProp)
        {
            bakedProp.managedReferenceValue = null;
        }

        private static bool IsBakeCurrent(SerializedProperty bakedProp, Type assetType, string loadKey)
        {
            if (bakedProp.managedReferenceValue == null)
            {
                return false;
            }

            Type valueType = bakedProp.managedReferenceValue.GetType();
            if (!valueType.IsGenericType ||
                valueType.GetGenericTypeDefinition() != typeof(AddressableScriptableObjectPublisherRegistrar<>))
            {
                return false;
            }

            Type[] args = valueType.GetGenericArguments();
            if (args.Length != 1 || args[0] != assetType)
            {
                return false;
            }

            FieldInfo keyField = valueType.GetField("addressableKey", BindingFlags.Instance | BindingFlags.NonPublic);
            if (keyField == null)
            {
                return false;
            }

            string existingKey = keyField.GetValue(bakedProp.managedReferenceValue) as string;
            return string.Equals(existingKey, loadKey, StringComparison.Ordinal);
        }
    }
}
