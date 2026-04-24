using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Editor
{
    [CustomEditor(typeof(AddressableScriptableObjectPublisherSO))]
    public sealed class AddressableScriptableObjectPublisherSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty assetRef = serializedObject.FindProperty("assetReference");
            EditorGUILayout.PropertyField(assetRef);

            SerializedProperty baked = serializedObject.FindProperty("bakedRegistrar");
            var so = (AddressableScriptableObjectPublisherSO)target;

            if (baked.managedReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Not baked. Assign an Addressable ScriptableObject, then click Rebuild.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Baked registrar", baked.managedReferenceValue.GetType().FullName);
            }

            EditorGUI.BeginDisabledGroup(so.AssetReference == null || string.IsNullOrEmpty(so.AssetReference.AssetGUID));
            if (GUILayout.Button("Rebuild"))
            {
                AddressableScriptableObjectPublisherSORebaker.Rebake(so);
                serializedObject.Update();
            }

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
