using Scaffold.AppFlow;
using Scaffold.Ugs.Container;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class UgsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            new UgsInstaller().Install(builder);
        }
    }
}
