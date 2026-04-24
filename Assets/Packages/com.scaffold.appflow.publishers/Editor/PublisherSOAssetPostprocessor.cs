using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEditor;

namespace Scaffold.AppFlow.Publishers.Editor
{
    public sealed class PublisherSOAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets == null || importedAssets.Length == 0)
            {
                return;
            }

            foreach (string path in importedAssets)
            {
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AddressableScriptableObjectPublisherSO publisherSo =
                    AssetDatabase.LoadAssetAtPath<AddressableScriptableObjectPublisherSO>(path);
                if (publisherSo != null)
                {
                    AddressableScriptableObjectPublisherSORebaker.RebakeIfStale(publisherSo, force: false);
                }
            }
        }
    }
}
