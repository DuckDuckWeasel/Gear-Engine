using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Race.Bootstrap
{
    public sealed class RaceScope : SceneFoundationScope
    {
        [Header("Gear mechanics")]
        [FormerlySerializedAs("boardConfig")]
        [SerializeField]
        private BoardRulesSO boardRules;

        [Header("Bootstrap")]
        [SerializeField]
        private RaceBootstrap sceneBootstrap;

        [Header("Feature Toggles")]
        [SerializeField]
        private GearEngineFeatureToggleSO featureToggle;

        [Header("Simulation")]
        [SerializeField]
        private SplineCarRunnerConfigSO splineCarRunnerConfig;

        protected override void ValidateSceneAssignments()
        {
            RequireBoardConfig();
            RequireSceneBootstrap();

            if (splineCarRunnerConfig == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign SplineCarRunnerConfigSO.");
            }

            if (featureToggle == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign featureToggle.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.Register<EmptyInventoryService>(Lifetime.Singleton).As<IInventoryService>();
            builder.RegisterInstance<IBoardSlotCapacityProvider>(new UnlimitedBoardSlotCapacityProvider());
            builder.RegisterInstance(boardRules);
            builder.RegisterInstance(featureToggle);
            new GearMechanicsInstaller().Install(builder);

            builder.RegisterInstance(splineCarRunnerConfig);
            builder.Register<TrackSimulationFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<RaceManagerService>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<SplineCarRunnerService>(Lifetime.Singleton).AsSelf();

            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void RequireBoardConfig()
        {
            if (boardRules == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign boardRules.");
            }
        }

        private void RequireSceneBootstrap()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign sceneBootstrap.");
            }
        }
    }
}
