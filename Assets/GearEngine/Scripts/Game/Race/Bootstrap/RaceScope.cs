using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.SceneFoundation.Bootstrap;
using Sirenix.OdinInspector;
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

        [Header("Simulation Config (determines pipeline)")]
        [InlineEditor]
        [SerializeField]
        private SimulationConfigBase simulationConfig;

        protected override void ValidateSceneAssignments()
        {
            RequireBoardConfig();
            RequireSceneBootstrap();

            if (simulationConfig == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign a SimulationConfigBase asset.");
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

            new CarTrackInstaller().Install(builder, simulationConfig);

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
