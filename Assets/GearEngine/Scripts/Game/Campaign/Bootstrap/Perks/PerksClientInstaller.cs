using Scaffold.AppFlow;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.Perks
{
    public sealed class PerksClientInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<PerksClientModule>(Lifetime.Singleton)
                .AsSelf()
                .As<IPerksClientModule>()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
