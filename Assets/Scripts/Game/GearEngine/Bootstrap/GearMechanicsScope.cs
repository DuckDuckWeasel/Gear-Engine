using Scaffold.Events.Contracts;
using Scaffold.Events;
using VContainer;
using VContainer.Unity;

namespace Game.GearEngine
{
    public class GearMechanicsScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventController>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<GridManager>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Register Nodes
            builder.Register<CoreGearNode>(Lifetime.Transient);
            builder.Register<BaseGearNode>(Lifetime.Transient);
            builder.Register<AuraGearNode>(Lifetime.Transient);

            // Register Factories and Services
            builder.Register<GearMergeService>(Lifetime.Singleton);
            builder.Register<GearNodeFactory>(Lifetime.Singleton);
            builder.Register<GearViewFactory>(Lifetime.Singleton);

            // Note: Abilities are no longer registered in DI. 
            // They are ScriptableObjects injected via GearConfig setups within Unity.

            builder.RegisterComponentInHierarchy<GearBootstrap>();
        }
    }
}
