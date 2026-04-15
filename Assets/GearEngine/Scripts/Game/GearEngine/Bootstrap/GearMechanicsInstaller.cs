using System;
using VContainer;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Services;
using UnityEngine;

namespace GearEngine.GearEngine.Bootstrap
{
    public sealed class GearMechanicsInstaller
    {
        public GearMechanicsInstaller(BoardConfigSO boardConfig, GearEngineFeatureToggleSO featureToggle = null)
        {
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.featureToggle = featureToggle;
        }

        private readonly BoardConfigSO boardConfig;
        private readonly GearEngineFeatureToggleSO featureToggle;

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(boardConfig);
            builder.RegisterInstance(featureToggle);

            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<DragService>(Lifetime.Singleton).As<IDragService>();
            
            // New Generic Services
            builder.Register<GearTransferService>(Lifetime.Singleton).As<IGearTransferService>();
            builder.Register<GearTrashService>(Lifetime.Singleton).As<IGearTrashService>();
        }
    }
}

