using System;
using GearEngine.SceneFoundation.Bootstrap;
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

        protected override void ValidateSceneAssignments()
        {
            RequireSceneBootstrap();
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
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
