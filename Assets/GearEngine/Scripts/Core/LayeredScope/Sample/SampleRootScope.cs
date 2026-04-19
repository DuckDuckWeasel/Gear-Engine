using GearEngine.LayeredScope;
using VContainer;
using VContainer.Unity;

namespace GearEngine.LayeredScope.Sample
{
    public sealed class SampleRootScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            LayeredScopeInstaller.Install(builder);
        }
    }
}
