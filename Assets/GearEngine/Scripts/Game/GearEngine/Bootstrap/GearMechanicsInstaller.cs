using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using UnityEngine;
using VContainer;

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

        public void Install(IContainerBuilder builder, GearInventoryLoadoutData inventoryLoadout, GearBoardLoadoutData boardLoadout)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            inventoryLoadout ??= GearInventoryLoadoutData.Empty();
            boardLoadout ??= new GearBoardLoadoutData();

            GearEngineFeatureToggleSO toggle = featureToggle;
            if (toggle == null)
            {
                Debug.LogWarning("[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");
                toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            }

            builder.RegisterInstance(boardConfig);
            builder.RegisterInstance(toggle);
            builder.RegisterInstance(boardLoadout);

            builder.Register<GridManager>(Lifetime.Singleton).As<IGridManager, VContainer.Unity.ITickable>();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<Merge.GridMergeService>(Lifetime.Singleton).As<Merge.IGridMergeService>();
            builder.Register<Services.GridSwapService>(Lifetime.Singleton).As<Services.IGridSwapService>();
            builder.Register<GearNodeFactory>(Lifetime.Singleton).As<IGearNodeFactory>();
            builder.Register<DragService>(Lifetime.Singleton).As<IDragService>();

            builder.RegisterInstance<IInventoryService>(new InventoryService(inventoryLoadout));
            builder.Register<BoardService>(Lifetime.Singleton).As<IBoardService>();
            builder.Register<GearPresentationTransferService>(Lifetime.Singleton).As<IGearPresentationTransferService>();
        }
    }
}
