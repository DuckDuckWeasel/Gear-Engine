using System;
using Game.GearEngine.Presentation;
using Scaffold.Addressables.Container;
using Scaffold.Navigation;
using Scaffold.Navigation.Container;
using Scaffold.Scope;
using Scaffold.Scope.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public class GearMechanicsScope : LifetimeScope
    {
        [Header("Navigation")]
        [SerializeField] private NavigationSettings navigationSettings;
        [SerializeField] private Transform navigationViewHolder;

        [Header("Gear mechanics")]
        [SerializeField] private BoardConfigSO boardConfig;

        [Header("Feature Toggles")]
        [SerializeField] private GearEngineFeatureToggleSO featureToggle;

        [Header("Optional test launcher")]
        [SerializeField] private GearTestSceneBootstrap sceneBootstrap;

        protected override void Configure(IContainerBuilder builder)
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

            new AddressablesInstaller().Install(builder);
            builder.RegisterInstance(navigationSettings);
            new NavigationInstaller(navigationViewHolder).Install(builder);

            var installer = new GearMechanicsInstaller(boardConfig);
            installer.Install(builder);

            if (featureToggle != null)
            {
                builder.RegisterInstance(featureToggle);
            }

            if (sceneBootstrap != null)
            {
                builder.RegisterComponent(sceneBootstrap);
            }
        }
    }
}
