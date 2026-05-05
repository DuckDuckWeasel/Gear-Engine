using Scaffold.AppFlow;
using Scaffold.CloudCode.Container;
using Scaffold.LiveOps.Container;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    // todo: Cloud Code + LiveOps only; catalog Addressables load in FoundationLayer via layer asset publishers (AssetPublisherDefinition).
    public sealed class LiveOpsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            new CloudCodeInstaller().Install(builder);
            new LiveOpsInstaller().Install(builder);
        }
    }
}
