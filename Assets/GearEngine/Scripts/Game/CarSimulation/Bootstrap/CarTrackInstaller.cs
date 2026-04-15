using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using VContainer;
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
            builder.Register<UnityRaceRandom>(Lifetime.Singleton).As<IRaceRandom>();
            builder.Register<TrackSimulationRunner>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
