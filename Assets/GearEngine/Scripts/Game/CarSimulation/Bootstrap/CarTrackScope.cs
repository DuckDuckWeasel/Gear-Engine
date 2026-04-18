using System;
using GearEngine.SceneFoundation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
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
        
        [Header("Simulation Config")]
        [SerializeField]
        private SplineCarRunnerConfigSO splineCarRunnerConfig;

        protected override void ValidateSceneAssignments()
        {
            RequireSceneBootstrap();
            if (splineCarRunnerConfig == null)
            {
                throw new InvalidOperationException("[CarTrackScope] Assign SplineCarRunnerConfigSO.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(splineCarRunnerConfig);
            new CarTrackInstaller().Install(builder);
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
