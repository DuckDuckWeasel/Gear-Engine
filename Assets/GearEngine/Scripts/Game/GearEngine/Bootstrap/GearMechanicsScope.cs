using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearMechanicsScope : SceneFoundationScope
    {
        [Header("Gear mechanics")]
        [FormerlySerializedAs("boardConfig")]
        [SerializeField]
        private BoardRulesSO boardRules;

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
            if (boardRules == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign boardRules.");
            }

            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign sceneBootstrap.");
            }

            if (gearStartData == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign gearStartData.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            new GearMechanicsInstaller(
                boardRules,
                featureToggle,
                gearStartData.GetInventoryLoadoutData(),
                gearStartData.GetBoardLoadoutData()).Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }
}
