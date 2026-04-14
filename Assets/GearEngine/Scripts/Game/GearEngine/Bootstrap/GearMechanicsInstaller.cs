using System;
using VContainer;

namespace GearEngine.GearEngine.Bootstrap
{
    public sealed class GearMechanicsInstaller
    {
        public GearMechanicsInstaller(BoardConfigSO boardConfig)
        {
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
        }

        private readonly BoardConfigSO boardConfig;

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(boardConfig);

            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<DragService>(Lifetime.Singleton).As<IDragService>();
        }
    }
}
