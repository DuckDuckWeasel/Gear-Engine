using VContainer;
using VContainer.Unity;

namespace Scaffold.VisualScripting.Unity
{
    [UnityEngine.DisallowMultipleComponent]
    public sealed class BlackboardLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new BlackboardRuntimeInstaller().Install(builder);
            builder.RegisterComponentInHierarchy<BlackboardBehaviour>();
        }
    }
}
