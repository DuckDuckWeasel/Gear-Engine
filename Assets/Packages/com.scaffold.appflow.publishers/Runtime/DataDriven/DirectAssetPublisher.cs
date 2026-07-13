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
}
