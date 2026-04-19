using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
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

        [Header("Gear loadout")]
        [SerializeField]
        private GearEngineStartData gearStartData;

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

            if (gearStartData == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign gearStartData.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            new GearMechanicsInstaller(
                boardRules,
                featureToggle,
                gearStartData.GetInventoryLoadoutData(),
                gearStartData.GetBoardLoadoutData()).Install(builder);
            
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
