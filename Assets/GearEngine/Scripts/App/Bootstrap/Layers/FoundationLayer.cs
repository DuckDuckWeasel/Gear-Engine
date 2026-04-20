using System;
using GearEngine.LayeredScope;
using Scaffold.Addressables.Container;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
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
            InstallCrossLayer(builder);
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
            new EventsInstaller().Install(builder);
        }

        private void InstallCrossLayer(IContainerBuilder builder)
        {
            builder.Register<CrossLayerObjectResolver>(Lifetime.Singleton)
                .As<Scaffold.Scope.Contracts.ICrossLayerObjectResolver>()
                .AsSelf();

            builder.RegisterBuildCallback(container =>
            {
                try
                {
                    Scaffold.Scope.Contracts.ICrossLayerObjectResolver cross = container.Resolve<Scaffold.Scope.Contracts.ICrossLayerObjectResolver>();
                    cross.Reset();
                    cross.RegisterScope(container);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FoundationLayer] Cross-layer registration failed: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }
    }
}
