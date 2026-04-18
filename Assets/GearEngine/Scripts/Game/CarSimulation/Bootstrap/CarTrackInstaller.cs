using GearEngine.CarSimulation.Simulation;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Bootstrap
{
    public sealed class CarTrackInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Register<TrackSimulationFactory>(Lifetime.Singleton);
            builder.Register<RaceSessionRunner>(Lifetime.Singleton).As<IRaceSessionRunner, ITickable>();
        }
    }
}
