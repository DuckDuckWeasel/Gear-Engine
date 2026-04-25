using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.Addressables.Contracts;
using Scaffold.AppFlow;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Scaffold.AppFlow.Publishers.Addressables
{
    [UnityEngine.Scripting.Preserve]
    public sealed class AddressableLabelPublisher<T> : IAsyncInitializable
        where T : UnityEngine.Object
    {
        private readonly ILayerPublisher publisher;
        private readonly IAddressablesAssetClient client;
        private readonly string labelString;

        public AddressableLabelPublisher(
            ILayerPublisher publisher,
            IAddressablesAssetClient client,
            string labelString)
        {
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.labelString = labelString ?? throw new ArgumentNullException(nameof(labelString));
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(labelString))
                {
                    throw new InvalidOperationException("Label string is empty.");
                }

                var label = new AssetLabelReference { labelString = labelString };
                IReadOnlyList<T> assets = await client.LoadAssetsByLabelAsync<T>(label, ct);
                publisher.PublishMany(assets);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[AddressableLabelPublisher<{typeof(T).Name}>] Failed to load and publish: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
