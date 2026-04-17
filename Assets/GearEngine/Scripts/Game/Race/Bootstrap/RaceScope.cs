using System;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Race.Bootstrap
{
    public sealed class RaceScope : SceneFoundationScope
    {
        [Header("Gear mechanics")]
        [SerializeField]
        private BoardConfigSO boardConfig;

        [Header("Bootstrap")]
        [SerializeField]
        private RaceBootstrap sceneBootstrap;

        [Header("Feature Toggles")]
        [SerializeField]
        private GearEngineFeatureToggleSO featureToggle;

        protected override void ValidateSceneAssignments()
        {
            RequireBoardConfig();
            RequireSceneBootstrap();
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            new GearMechanicsInstaller(boardConfig, featureToggle).Install(builder);
            new CarTrackInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void RequireBoardConfig()
        {
            if (boardConfig == null)
            {
                throw new InvalidOperationException("[RaceScope] Assign boardConfig.");
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
