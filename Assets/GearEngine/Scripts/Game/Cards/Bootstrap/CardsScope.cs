using System;
using GearEngine.SceneFoundation.Bootstrap;
using Scaffold.LiveOps;
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
            RequireParentLiveOps();
        }

        protected override void InstallFeatureServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(catalog);
            builder.Register<CardSampleViewModel>(Lifetime.Transient);
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

        private void RequireParentLiveOps()
        {
            if (TryResolveParentLiveOps() == null)
            {
                throw new InvalidOperationException(
                    "[CardsScope] No ILiveOpsService in parent LifetimeScope. Set this scope's Parent to the Meta application root (or any scope that registered LiveOps + LiveOpsLayer).");
            }
        }

        private ILiveOpsService TryResolveParentLiveOps()
        {
            for (LifetimeScope p = Parent; p != null; p = p.Parent)
            {
                if (p.Container == null)
                {
                    continue;
                }

                try
                {
                    return p.Container.Resolve<ILiveOpsService>();
                }
                catch (VContainerException)
                {
                    // Parent chain continues
                }
            }

            return null;
        }
    }
}
