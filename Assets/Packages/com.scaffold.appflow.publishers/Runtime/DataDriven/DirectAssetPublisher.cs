using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scaffold.AppFlow;
using UnityEngine;
using VContainer;

namespace Scaffold.AppFlow.Publishers.DataDriven
{
    [UnityEngine.Scripting.Preserve]
    public sealed class DirectAssetPublisher<T> : AssetPublisherBase<T>
        where T : UnityEngine.Object
    {
        private readonly T asset;

        public DirectAssetPublisher(ILayerPublisher layerPublisher, T value)
            : base(layerPublisher)
        {
            asset = value;
        }

        protected override Task<T> LoadAssetAsync(CancellationToken ct)
        {
            return Task.FromResult(asset);
        }
    }

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
