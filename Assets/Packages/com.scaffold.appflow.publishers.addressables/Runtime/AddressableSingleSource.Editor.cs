#if UNITY_EDITOR
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    public partial class AddressableSingleSource
    {
        /// <summary>Editor: points this source at any Addressable asset by entry GUID (yaml migration / bootstrap).</summary>
        public void SetAssetByGuidForEditor(string addressableObjectGuid)
        {
            assetReference = new AssetReference(addressableObjectGuid);
        }

        public bool IsConfigured => AssetReference != null && !string.IsNullOrEmpty(AssetReference.AssetGUID);

        public IPublisherRegistrar Bake()
        {
            IPublisherRegistrar r = AddressableBakeUtility.BakeAddressableSingle(this);
            bakedAssetTypeAqn = string.Empty;
            if (r != null && AssetReference != null)
            {
                (string key, System.Type type)? res = AddressableBakeUtility.ResolveSingle(AssetReference);
                if (res != null)
                {
                    bakedAssetTypeAqn = res.Value.type.AssemblyQualifiedName;
                }
            }

            return r;
        }
    }
}
#endif
