using UnityEditor;

namespace Scaffold.AppFlow.Publishers.Addressables.Editor
{
    /// <summary>Invalidates <see cref="AddressableBakeUtility"/> caches when addressable data or the editor domain changes.</summary>
    internal static class AddressableBakeCacheInvalidator
    {
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            AssemblyReloadEvents.afterAssemblyReload += AddressableBakeUtility.InvalidateCache;
        }
    }
}
