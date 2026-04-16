using System;
using GearEngine.SceneFoundation.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Cards.Bootstrap
{
    public sealed class CardsScope : SceneFoundationScope
    {
        [Header("Cards")]
        [SerializeField]
        private CardCatalogSO catalog;

        [Header("Bootstrap")]
        [SerializeField]
        private CardsBootstrap sceneBootstrap;

        protected override void ValidateSceneAssignments()
        {
            RequireCatalog();
            RequireSceneBootstrap();
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(catalog);
            builder.RegisterComponent(sceneBootstrap).AsImplementedInterfaces().AsSelf();
        }

        private void RequireCatalog()
        {
            if (catalog == null)
            {
                throw new InvalidOperationException("[CardsScope] Assign catalog.");
            }
        }

        private void RequireSceneBootstrap()
        {
            if (sceneBootstrap == null)
            {
                throw new InvalidOperationException("[CardsScope] Assign sceneBootstrap.");
            }
        }
    }
}
