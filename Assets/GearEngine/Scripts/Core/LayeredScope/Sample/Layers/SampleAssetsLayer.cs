using System.Threading;
using System.Threading.Tasks;
using GearEngine.LayeredScope;
using UnityEngine;
using VContainer;

namespace GearEngine.LayeredScope.Sample.Layers
{
    public interface ISampleAssetGateway
    {
        Task<string> LoadAsync(string key, CancellationToken ct);
    }

    internal sealed class SampleAssetGateway : ISampleAssetGateway, IAsyncInitializable
    {
        public async Task InitializeAsync(CancellationToken ct)
        {
            Debug.Log("[SampleAssetGateway] warming…");
            await Task.Delay(200, ct);
            Debug.Log("[SampleAssetGateway] ready.");
        }

        public Task<string> LoadAsync(string key, CancellationToken ct) => Task.FromResult($"asset:{key}");
    }

    public sealed class SampleAssetsLayer : IScopeLayer
    {
        public string Name => "SampleAssets";
        public void Install(IContainerBuilder builder)
        {
            builder.Register<SampleAssetGateway>(Lifetime.Singleton)
                .As<ISampleAssetGateway>()
                .As<IAsyncInitializable>();
        }
    }
}
