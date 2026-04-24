using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;

namespace Scaffold.AppFlow.Publishers.Editor
{
    public static class PublisherSORebakeMenu
    {
        [MenuItem("Tools/Scaffold/AppFlow/Rebake All Publisher SOs")]
        public static void RebakeAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:AddressableScriptableObjectPublisherSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AddressableScriptableObjectPublisherSO so =
                    AssetDatabase.LoadAssetAtPath<AddressableScriptableObjectPublisherSO>(path);
                if (so != null)
                {
                    AddressableScriptableObjectPublisherSORebaker.Rebake(so);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
