using GearEngine.LayeredScope.Internal;
using VContainer;

namespace GearEngine.LayeredScope
{
    public static class LayeredScopeInstaller
    {
        public static void Install(IContainerBuilder builder)
        {
            var proxy = new LayerResolverProxy();
            builder.RegisterInstance<LayerResolverProxy, ILayerResolver>(proxy);
        }
    }
}
