using Scaffold.AppFlow;
using Scaffold.CloudCode;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    // todo: Cloud Code + LiveOps only; catalog Addressables load in FoundationLayer via layer asset publishers (AssetPublisherDefinition).
    public sealed class LiveOpsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<LazyCloudCodeService>(Lifetime.Singleton).As<ICloudCodeService>();
            new ResilientLiveOpsInstaller().Install(builder);
        }
    }
}
