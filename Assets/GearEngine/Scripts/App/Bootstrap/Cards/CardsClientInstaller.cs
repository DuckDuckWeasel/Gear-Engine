using GearEngine.LayeredScope;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Cards
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
