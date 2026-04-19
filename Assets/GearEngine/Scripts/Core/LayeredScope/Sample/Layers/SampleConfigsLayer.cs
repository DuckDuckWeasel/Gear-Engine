using System.Threading;
using System.Threading.Tasks;
using GearEngine.LayeredScope;
using UnityEngine;
using VContainer;

namespace GearEngine.LayeredScope.Sample.Layers
{
    public sealed class SampleConfig { public int Value; }

    public interface ISampleConfigService { SampleConfig Current { get; } }

    internal sealed class SampleConfigService : ISampleConfigService, IAsyncInitializable
    {
        private readonly ISampleAssetGateway gateway;
        public SampleConfig Current { get; private set; }

        public SampleConfigService(ISampleAssetGateway gateway) => this.gateway = gateway;

        public async Task InitializeAsync(CancellationToken ct)
        {
            Debug.Log("[SampleConfigService] loading via gateway…");
            string raw = await gateway.LoadAsync("config", ct);
            Current = new SampleConfig { Value = raw.Length };
            Debug.Log($"[SampleConfigService] ready (value={Current.Value}).");
        }
    }

    public sealed class SampleConfigsLayer : IScopeLayer
    {
        public string Name => "SampleConfigs";
        public void Install(IContainerBuilder builder)
        {
            builder.Register<SampleConfigService>(Lifetime.Singleton)
                .As<ISampleConfigService>()
                .As<IAsyncInitializable>();
        }
    }
}
