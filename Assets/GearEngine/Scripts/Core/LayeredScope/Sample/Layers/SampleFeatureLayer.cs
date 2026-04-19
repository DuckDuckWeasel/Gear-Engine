using System;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.LayeredScope;
using UnityEngine;
using VContainer;

namespace GearEngine.LayeredScope.Sample.Layers
{
    public sealed class SampleAsset { public string Payload; }

    internal sealed class SampleFeatureService : IAsyncInitializable, IAsyncDisposable
    {
        private readonly SampleAsset asset;
        private readonly ISampleConfigService config;
        private readonly ILayerResolver layered;

        public SampleFeatureService(SampleAsset asset, ISampleConfigService config, ILayerResolver layered)
        {
            this.asset = asset;
            this.config = config;
            this.layered = layered;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            Debug.Log($"[SampleFeatureService] init asset='{asset.Payload}', config={config.Current.Value}, top resolves gateway? {layered.TryResolve(out ISampleAssetGateway _)}");
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Debug.Log("[SampleFeatureService] async dispose.");
            return default;
        }
    }

    public sealed class SampleFeatureLayer : IAsyncScopeLayer
    {
        private SampleAsset prepared;

        public string Name => "SampleFeature";

        public async Task PrepareAsync(IObjectResolver parent, CancellationToken ct)
        {
            var gateway = parent.Resolve<ISampleAssetGateway>();
            string raw = await gateway.LoadAsync("feature.payload", ct);
            prepared = new SampleAsset { Payload = raw };
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(prepared);
            builder.Register<SampleFeatureService>(Lifetime.Singleton)
                .As<IAsyncInitializable>()
                .As<IAsyncDisposable>();
        }
    }
}
