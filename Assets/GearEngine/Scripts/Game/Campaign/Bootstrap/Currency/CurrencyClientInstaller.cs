using Scaffold.AppFlow;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Currency.Bootstrap
{
    public sealed class CurrencyClientInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<CurrencyClientModule>(Lifetime.Singleton)
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
