using GearEngine.App.Bootstrap.Publishers.DataDriven;
using UnityEditor;

namespace GearEngine.App.Bootstrap.Editor
{
    public static class PublisherSORebakeMenu
    {
        [MenuItem("Tools/Bootstrap/Rebake All Publisher SOs")]
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
