using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.AppFlow;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [UnityEngine.Scripting.Preserve]
    public sealed class DirectAssetListPublisher<T> : IAsyncInitializable
        where T : UnityEngine.Object
    {
        private readonly ILayerPublisher layerPublisher;
        private readonly IReadOnlyList<T> assets;

        public DirectAssetListPublisher(ILayerPublisher layerPublisher, IReadOnlyList<T> assets)
        {
            this.layerPublisher = layerPublisher ?? throw new ArgumentNullException(nameof(layerPublisher));
            this.assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return Task.FromCanceled(ct);
            }

            try
            {
                layerPublisher.PublishMany(assets);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(DirectAssetListPublisher<T>)}] PublishMany failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
