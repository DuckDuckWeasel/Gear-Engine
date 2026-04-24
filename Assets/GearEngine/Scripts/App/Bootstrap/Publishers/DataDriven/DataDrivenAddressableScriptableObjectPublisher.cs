using System;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Publishers.DataDriven
{
    // todo: Addressable-backed publisher with key supplied at VContainer registration (data-driven path).
    [UnityEngine.Scripting.Preserve]
    public sealed class DataDrivenAddressableScriptableObjectPublisher<T> : AssetPublisherBase<T> where T : ScriptableObject
    {
        public DataDrivenAddressableScriptableObjectPublisher(ILayerPublisher layerPublisher, IAddressablesAssetClient client, string addressableKey) : base(layerPublisher)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(addressableKey))
            {
                throw new ArgumentException("Addressable key cannot be null or empty.", nameof(addressableKey));
            }

            this.addressableKey = addressableKey;
        }

        private readonly IAddressablesAssetClient client;
        private readonly string addressableKey;

        protected override Task<T> LoadAssetAsync(CancellationToken ct)
        {
            return client.LoadAssetAsync<T>(addressableKey, ct);
        }
    }
}
