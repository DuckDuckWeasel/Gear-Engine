using Scaffold.AppFlow;
using Scaffold.Ugs.Container;
using Scaffold.Analytics;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class UgsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            new UgsInstaller().Install(builder);
            new AnalyticsInstaller().Install(builder);
        }
    }
}
