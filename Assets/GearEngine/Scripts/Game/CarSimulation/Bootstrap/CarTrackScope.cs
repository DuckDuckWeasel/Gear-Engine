using System;
using Scaffold.Addressables.Container;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
using Scaffold.Scope.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField] private NavigationSettings navigationSettings;
        [SerializeField] private Transform navigationViewHolder;

        [Header("Optional test launcher")]
        [SerializeField] private CarTrackBootstrap sceneBootstrap;

        [Header("Debug")]
        [SerializeField] private MonoBehaviour debugComponent;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCrossLayer(builder);
            InstallInfra(builder);
            new CarTrackInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
            if (debugComponent != null)
            {
                builder.RegisterComponent(debugComponent).AsImplementedInterfaces().AsSelf();
            }
        }

        private void RegisterCrossLayer(IContainerBuilder builder)
        {
            builder.Register<CrossLayerObjectResolver>(Lifetime.Singleton)
                .As<ICrossLayerObjectResolver>()
                .AsSelf();
            builder.RegisterBuildCallback(RegisterCrossLayerOnBuild);
        }

        private void RegisterCrossLayerOnBuild(IObjectResolver container)
        {
            try
            {
                ICrossLayerObjectResolver cross = container.Resolve<ICrossLayerObjectResolver>();
                cross.Reset();
                cross.RegisterScope(container);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarTrackScope] Cross-layer registration failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void InstallInfra(IContainerBuilder builder)
        {
            builder.RegisterInstance(navigationSettings);
            new AddressablesInstaller().Install(builder);
            new NavigationInstaller(navigationViewHolder).Install(builder);
            new EventsInstaller().Install(builder);
        }
    }
}
