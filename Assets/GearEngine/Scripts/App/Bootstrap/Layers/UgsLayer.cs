using Scaffold.AppFlow;
using Scaffold.Analytics;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class UgsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<OfflineSessionState>(Lifetime.Singleton);
            builder.Register<Scaffold.Ugs.Ugs>(Lifetime.Singleton).AsSelf();
            builder.Register<AnalyticsService>(Lifetime.Singleton)
                .AsSelf()
                .As<IAnalyticsService>();
            builder.Register<UgsAnalyticsInitializer>(Lifetime.Singleton)
                .As<IAsyncInitializable>();
        }
    }
}
