using System;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using Scaffold.Addressables.Container;
using Scaffold.Events.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
using Scaffold.Scope.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Race.Bootstrap
{
    public sealed class RaceScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;

        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Gear mechanics")]
        [SerializeField]
        private BoardConfigSO boardConfig;

        [Header("Bootstrap")]
        [SerializeField]
        private RaceBootstrap sceneBootstrap;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateScopeAssignments();
            BuildCrossLayerRegistration(builder);
            InstallAddressablesAndNavigation(builder);
            new GearMechanicsInstaller(boardConfig).Install(builder);
            new CarTrackInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void ValidateScopeAssignments()
        {
            RequireBoardConfig();
            RequireNavigationSettings();
            RequireNavigationViewHolder();
            RequireSceneBootstrap();
        }

        private void RequireBoardConfig()
        {
            if (boardConfig == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign boardConfig.");
            }
        }

        private void RequireNavigationSettings()
        {
            if (navigationSettings == null)
            {
                throw new InvalidOperationException(
                    "[RaceScope] Assign navigationSettings (e.g. Assets/Data/Navigation/Navigation Settings.asset).");
            }
        }

        private void RequireNavigationViewHolder()
        {
            if (navigationViewHolder == null)
            {
                throw new InvalidOperationException(
                    "[RaceScope] Assign navigationViewHolder to the transform that parents the scene RaceView.");
            }
        }

        private void RequireSceneBootstrap()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign sceneBootstrap.");
            }
        }

        private void BuildCrossLayerRegistration(IContainerBuilder builder)
        {
            builder.Register<CrossLayerObjectResolver>(Lifetime.Singleton)
                .As<ICrossLayerObjectResolver>()
                .AsSelf();

            builder.RegisterBuildCallback(container =>
            {
                try
                {
                    ICrossLayerObjectResolver cross = container.Resolve<ICrossLayerObjectResolver>();
                    cross.Reset();
                    cross.RegisterScope(container);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RaceScope] Cross-layer registration failed: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private void InstallAddressablesAndNavigation(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
            new EventsInstaller().Install(builder);
        }
    }
}
