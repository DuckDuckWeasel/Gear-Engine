using System;
using GearEngine.GearEngine.Presentation;
using Scaffold.Addressables.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
using Scaffold.Scope.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearMechanicsScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField]
        private NavigationSettings navigationSettings;
        [SerializeField]
        private Transform navigationViewHolder;

        [Header("Gear mechanics")]
        [SerializeField]
        private BoardConfigSO boardConfig;

        [Header("Optional test launcher")]
        [SerializeField]
        private GearTestSceneBootstrap sceneBootstrap;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateScopeAssignments();
            BuildCrossLayerRegistration(builder);
            InstallAddressablesAndNavigation(builder);
            InstallGearMechanics(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void ValidateScopeAssignments()
        {
            if (boardConfig == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign boardConfig.");
            }

            if (navigationSettings == null)
            {
                throw new InvalidOperationException(
                    "GearMechanicsScope: assign navigationSettings (e.g. Assets/Data/Navigation/Navigation Settings.asset). Run GearEngine/Generate Navigation Assets if the Gear Engine view config is missing.");
            }

            if (navigationViewHolder == null)
            {
                throw new InvalidOperationException(
                    "GearMechanicsScope: assign navigationViewHolder to a transform that parents the GearEngineView (context view). Usually the GearEngine_Root transform.");
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
                    Debug.LogError($"[GearMechanicsScope] Cross-layer registration failed: {ex.Message}\n{ex.StackTrace}");
                }
            });
        }

        private void InstallAddressablesAndNavigation(IContainerBuilder builder)
        {
            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);
        }

        private void InstallGearMechanics(IContainerBuilder builder)
        {
            var installer = new GearMechanicsInstaller(boardConfig);
            installer.Install(builder);
        }
    }
}
