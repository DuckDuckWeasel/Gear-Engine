using Scaffold.LayeredScope;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.Cards
{
    public sealed class CardsClientInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CardsClientModule>(Lifetime.Singleton)
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
