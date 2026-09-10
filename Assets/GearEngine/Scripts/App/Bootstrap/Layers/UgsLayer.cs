using Scaffold.AppFlow;
using Scaffold.Analytics;
using Scaffold.Ugs.Container;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class UgsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            new UgsInstaller().Install(builder);
            builder.Register<AnalyticsService>(Lifetime.Singleton)
                .AsSelf()
                .As<IAnalyticsService>();
            builder.Register<UgsAnalyticsInitializer>(Lifetime.Singleton)
                .As<IAsyncInitializable>();
        }
    }
}
