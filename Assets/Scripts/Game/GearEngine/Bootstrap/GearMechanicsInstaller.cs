using System;
using Scaffold.Events;
using VContainer;
using VContainer.Unity;

namespace Scaffold.GearEngine.Bootstrap
{
    /// <summary>
    /// Registers GearEngine services and scene-resident instances. No views or view models.
    /// </summary>
    public sealed class GearMechanicsInstaller
    {
        private readonly BoardConfigSO boardConfig;

        public GearMechanicsInstaller(BoardConfigSO boardConfig)
        {
            this.boardConfig = boardConfig ?? throw new ArgumentNullException(nameof(boardConfig));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(boardConfig);

            builder.Register<EventController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GearEngineService>(Lifetime.Singleton).As<IGearEngineService>();

            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
        }
    }
}
