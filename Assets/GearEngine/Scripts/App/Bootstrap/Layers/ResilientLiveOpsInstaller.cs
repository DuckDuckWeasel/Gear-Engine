using Scaffold.AppFlow;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class ResilientLiveOpsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<OfflineLiveOpsState>(Lifetime.Singleton);
            builder.Register<ResilientLiveOpsService>(Lifetime.Singleton)
                .As<ILiveOpsService>()
                .As<IAsyncInitializable>();
        }
    }
}
