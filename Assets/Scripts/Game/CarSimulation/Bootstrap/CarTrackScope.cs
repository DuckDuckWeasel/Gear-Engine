using UnityEngine;
using UnityEngine.Splines;
using VContainer;
using VContainer.Unity;

namespace Game.CarSimulation
{
    public class CarTrackScope : LifetimeScope
    {
        [SerializeField] private CarDefinition carDefinition;
        [SerializeField] private SplineContainer splineContainer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CarFactory>(Lifetime.Singleton);
            builder.RegisterInstance(carDefinition);
            builder.RegisterComponent(splineContainer);
            builder.RegisterEntryPoint<CarTrackBootstrap>();
        }
    }
}
