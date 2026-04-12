using VContainer;

namespace Game.CarSimulation
{
    public sealed class CarTrackInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Register<ITrackSimulationService, TrackSimulationService>(Lifetime.Singleton);
        }
    }
}
