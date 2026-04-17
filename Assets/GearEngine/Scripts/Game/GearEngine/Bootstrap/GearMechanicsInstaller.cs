using System;
using VContainer;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.GearEngine.Bootstrap
{
    public sealed class GearMechanicsInstaller
    {
        public GearMechanicsInstaller(BoardConfigSO boardConfig, GearEngineFeatureToggleSO featureToggle)
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

            builder.Register<GridManager>(Lifetime.Singleton).As<IGridManager, VContainer.Unity.ITickable>();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<Merge.GridMergeService>(Lifetime.Singleton).As<Merge.IGridMergeService>();
            builder.Register<Services.GridSwapService>(Lifetime.Singleton).As<Services.IGridSwapService>();
            builder.Register<GearNodeFactory>(Lifetime.Singleton).As<IGearNodeFactory>();
            builder.Register<DragService>(Lifetime.Singleton).As<IDragService>();
            
            builder.Register<GearTransferService>(Lifetime.Singleton).As<IGearTransferService>();
            builder.Register<GearTrashService>(Lifetime.Singleton).As<IGearTrashService>();
            builder.Register<InventoryService>(Lifetime.Singleton).As<IInventoryService>();

            // ViewModels
            builder.Register<GearInventoryViewModel>(Lifetime.Transient);
            builder.Register<BoardViewModel>(Lifetime.Transient);
            builder.Register<TrashZoneViewModel>(Lifetime.Transient);
        }
    }
}

