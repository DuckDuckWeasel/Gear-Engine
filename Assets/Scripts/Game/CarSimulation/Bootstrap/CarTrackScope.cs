using System;
using Scaffold.Addressables.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
using Scaffold.Scope.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public sealed class CarTrackScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField] private NavigationSettings navigationSettings;
        [SerializeField] private Transform navigationViewHolder;

        [Header("Optional test launcher")]
        [SerializeField] private CarTrackBootstrap sceneBootstrap;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateNavigationFields();
            RegisterCrossLayer(builder);
            InstallCarTrackNavigation(builder);
            new CarTrackInstaller().Install(builder);
            RegisterOptionalBootstrap(builder);
        }

        private void ValidateNavigationFields()
        {
            if (navigationSettings == null)
            {
                throw new InvalidOperationException(
                    "CarTrackScope: assign navigationSettings (e.g. Assets/Data/Navigation/Navigation Settings.asset). Run Game/Car Simulation/Generate Navigation Assets if the Car Simulation view config is missing.");
            }

            if (navigationViewHolder == null)
            {
                throw new InvalidOperationException(
                    "CarTrackScope: assign navigationViewHolder to a transform that parents the Track view (context view). Usually a child of CarTrack_LifetimeScope named NavigationViewHolder.");
            }
        }

        private void RegisterCrossLayer(IContainerBuilder builder)
        {
            builder.Register<CrossLayerObjectResolver>(Lifetime.Singleton)
                .As<ICrossLayerObjectResolver>()
                .AsSelf();
            builder.RegisterBuildCallback(TryRegisterCrossLayerScope);
        }

        private static void TryRegisterCrossLayerScope(IObjectResolver container)
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

        private void InstallCarTrackNavigation(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
        }

        private void RegisterOptionalBootstrap(IContainerBuilder builder)
        {
            if (sceneBootstrap != null)
            {
                builder.RegisterComponent(sceneBootstrap);
            }
        }
    }
}
