using System;
using GearEngine.SceneFoundation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackScope : SceneFoundationScope
    {
        [Header("Optional test launcher")]
        [SerializeField]
        private CarTrackBootstrap sceneBootstrap;

        [Header("Simulation Config (determines pipeline)")]
        [InlineEditor]
        [SerializeField]
        private SimulationConfigBase simulationConfig;

        protected override void ValidateSceneAssignments()
        {
            RequireSceneBootstrap();
            if (simulationConfig == null)
            {
                throw new InvalidOperationException("[CarTrackScope] Assign a SimulationConfigBase asset.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(simulationConfig);
            new CarTrackInstaller().Install(builder, simulationConfig);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void RequireSceneBootstrap()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[CarTrackScope] Assign sceneBootstrap.");
            }
        }
    }
}
