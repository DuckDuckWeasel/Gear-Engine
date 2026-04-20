using Scaffold.LayeredScope;
using Scaffold.Navigation.Contracts;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class LayerTopNavigationViewModelInjector : INavigationViewModelInjector
    {
        private readonly ILayerResolver layers;

        public LayerTopNavigationViewModelInjector(ILayerResolver layers)
        {
            this.layers = layers;
        }

        public void Inject(IViewController viewModel)
        {
            layers.Top.Inject(viewModel);
        }
    }
}
