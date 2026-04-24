using System;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
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

            if (featureToggle == null)
            {
                throw new InvalidOperationException("GearMechanicsScope: assign featureToggle.");
            }
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.Register<EmptyInventoryService>(Lifetime.Singleton).As<IInventoryService>();
            builder.RegisterInstance<IBoardSlotCapacityProvider>(new UnlimitedBoardSlotCapacityProvider());
            builder.RegisterInstance(boardRules);
            builder.RegisterInstance(featureToggle);
            new GearMechanicsInstaller().Install(builder);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }
    }
}
