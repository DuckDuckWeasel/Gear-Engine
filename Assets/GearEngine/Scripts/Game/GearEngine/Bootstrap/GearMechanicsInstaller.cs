using System;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using VContainer;

namespace GearEngine.GearEngine.Bootstrap
{
    public sealed class GearMechanicsInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Register<GridManager>(Lifetime.Singleton).As<IGridManager, VContainer.Unity.ITickable>();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<Merge.GridMergeService>(Lifetime.Singleton).As<Merge.IGridMergeService>();
            builder.Register<Services.GridSwapService>(Lifetime.Singleton).As<Services.IGridSwapService>();
            builder.Register<GearNodeFactory>(Lifetime.Singleton).As<IGearNodeFactory>();
            builder.Register<DragService>(Lifetime.Singleton).As<IDragService>();

            builder.Register<BoardService>(Lifetime.Singleton).As<IBoardService>();
            builder.Register<GearPresentationTransferService>(Lifetime.Singleton).As<IGearPresentationTransferService>();
        }
    }
}
