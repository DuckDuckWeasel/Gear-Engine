using System;
using Scaffold.AppFlow.Publishers.DataDriven;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    [Serializable]
    public partial class AddressableSingleSource : IAssetPublisherSource
    {
        [SerializeField]
        private AssetReference assetReference;

        /// <summary>Set by editor bake; used when the embedded registrar is missing so player can still register the closed generic.</summary>
        [SerializeField]
        [HideInInspector]
        private string bakedAssetTypeAqn = string.Empty;

        public AssetReference AssetReference => assetReference;

        /// <inheritdoc />
        public IPublisherRegistrar TryCreateRuntimeBakedRegistrar()
        {
            if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
            {
                return null;
            }

            string key = assetReference.RuntimeKey?.ToString();
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (string.IsNullOrEmpty(bakedAssetTypeAqn))
            {
                return null;
            }

            Type t = Type.GetType(bakedAssetTypeAqn, throwOnError: false);
            if (t == null || !typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                return null;
            }

            Type closed = typeof(AddressableSinglePublisherRegistrar<>).MakeGenericType(t);
            return (IPublisherRegistrar)Activator.CreateInstance(closed, key);
        }
    }
}
