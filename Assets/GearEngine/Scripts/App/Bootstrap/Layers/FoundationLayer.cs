using System;
using Scaffold.LayeredScope;
using Scaffold.Addressables.Container;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Navigation.Contracts;
using UnityEngine;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class FoundationLayer : IScopeLayer
    {
        public FoundationLayer(NavigationSettings navigationSettings, Transform navigationViewHolder)
        {
            this.navigationSettings = navigationSettings ?? throw new ArgumentNullException(nameof(navigationSettings));
            this.navigationViewHolder = navigationViewHolder ?? throw new ArgumentNullException(nameof(navigationViewHolder));
        }

        private readonly NavigationSettings navigationSettings;
        private readonly Transform navigationViewHolder;

        public void Install(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            builder.Register<LayerTopNavigationViewModelInjector>(Lifetime.Singleton).As<INavigationViewModelInjector>();
            new NavigationInstaller(navigationViewHolder, registerDefaultViewModelInjector: false).Install(builder);
            new EventsInstaller().Install(builder);
        }
    }
}
