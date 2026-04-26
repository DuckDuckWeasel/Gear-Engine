using UnityEditor;

namespace Scaffold.AppFlow.Publishers.Addressables.Editor
{
    /// <summary>Invalidates label-bake caches when addressable group assets or related editor assets change.</summary>
    internal sealed class AddressableBakeCachePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedToNewPaths,
            string[] movedFromOldPaths)
        {
            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (ShouldInvalidate(importedAssets[i]))
                {
                    AddressableBakeUtility.InvalidateCache();
                    return;
                }
            }

            for (int i = 0; i < deletedAssets.Length; i++)
            {
                if (ShouldInvalidate(deletedAssets[i]))
                {
                    AddressableBakeUtility.InvalidateCache();
                    return;
                }
            }

            int movedCount = System.Math.Min(movedToNewPaths.Length, movedFromOldPaths.Length);
            for (int i = 0; i < movedCount; i++)
            {
                if (ShouldInvalidate(movedToNewPaths[i]) || ShouldInvalidate(movedFromOldPaths[i]))
                {
                    AddressableBakeUtility.InvalidateCache();
                    return;
                }
            }
        }

        private static bool ShouldInvalidate(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (path.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".asset.meta", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (path.IndexOf("AddressableAssetGroup", System.StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}
