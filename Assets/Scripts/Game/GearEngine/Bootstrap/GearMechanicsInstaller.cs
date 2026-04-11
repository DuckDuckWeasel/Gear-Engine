using System;
using Scaffold.Events;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    /// <summary>
    /// Registers GearEngine services and scene-resident instances. No views or view models.
    /// </summary>
    public sealed class GearMechanicsInstaller
    {
        private readonly BoardConfigSO boardConfig;
        private readonly GearBootstrap bootstrap;
        private readonly GearInventoryLoadoutSO loadout;

        public GearMechanicsInstaller(
            BoardConfigSO boardConfig,
            GearBootstrap bootstrap,
            GearInventoryLoadoutSO loadout)
        {
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
            this.bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
            this.loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(boardConfig);
            builder.RegisterInstance(loadout);
            builder.RegisterInstance(bootstrap).As<IGearSceneElement>().AsSelf();

            builder.Register<EventController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<GearViewFactory>(Lifetime.Singleton);
        }
    }
}
