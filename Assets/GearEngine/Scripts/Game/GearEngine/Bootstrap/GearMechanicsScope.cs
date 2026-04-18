using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearMechanicsScope : SceneFoundationScope
    {
        [Header("Gear mechanics")]
        [SerializeField]
        private BoardConfigSO boardConfig;

        [Header("Gear start (inventory + board seed for tests)")]
        [SerializeField]
        private GearEngineStartData gearStartData;

        [Header("Feature Toggles")]
        [SerializeField]
        private GearEngineFeatureToggleSO featureToggle;

        [Header("Optional test launcher")]
        [SerializeField]
        private GearTestSceneBootstrap sceneBootstrap;

        protected override void ValidateSceneAssignments()
        {
            if (boardConfig == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign boardConfig.");
            }

            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign sceneBootstrap.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            GearEngineStartData start = gearStartData != null ? gearStartData : new GearEngineStartData();
            new GearMechanicsInstaller(boardConfig, featureToggle).Install(builder, start.GetInventoryLoadoutData(), start.GetBoardLoadoutData());
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }
}
