using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public class CarTrackScope : LifetimeScope
    {
        [SerializeField] private CarDefinition carDefinition;
        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private Track track;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CarFactory>(Lifetime.Singleton);
            builder.RegisterInstance(carDefinition);
            builder.RegisterInstance(trackDefinition);
            builder.RegisterComponent(track);
            builder.RegisterEntryPoint<CarTrackBootstrap>()
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterEntryPoint<CarAutoStartDriver>();
        }
    }
}
