using GearEngine.App.Bootstrap.Layers;
using Scaffold.LayeredScope;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using VContainer;
using VContainer.Unity;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class LayerTopNavigationViewModelInjectorTests
    {
        [Test]
        public void Inject_UsesLayerResolverTopContainer()
        {
            using LifetimeScope inner = LifetimeScope.Create(static builder =>
            {
                builder.RegisterInstance("from-top");
            }, "InjectorTestInner");

            var layers = new FakeLayerResolver(inner.Container);
            var injector = new LayerTopNavigationViewModelInjector(layers);
            var viewModel = new StubViewModel();
            injector.Inject(viewModel);

            Assert.That(viewModel.ResolvedTag, Is.EqualTo("from-top"));
        }

        private sealed class FakeLayerResolver : ILayerResolver
        {
            public FakeLayerResolver(IObjectResolver top)
            {
                Top = top;
            }

            public IObjectResolver Top { get; }

            public bool TryResolve<T>(out T value)
            {
                return Top.TryResolve(out value);
            }

            public T Resolve<T>()
            {
                return Top.Resolve<T>();
            }
        }

        private sealed class StubViewModel : IViewController
        {
            [Inject]
            public string ResolvedTag { get; set; }

            public void Bind(INavigation navigation)
            {
            }

            public void Close()
            {
            }
        }
    }
}
