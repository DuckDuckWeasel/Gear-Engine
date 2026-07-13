using Scaffold.Input.Contracts;
using VContainer;
using VContainer.Unity;

namespace Scaffold.Input.Container
{
    public class InputFilterInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<InputFilterService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
