using VContainer.Unity;

namespace Game.CarSimulation
{
    /// <summary>
    /// Optional entry point for test scenes: starts the car immediately after bootstrap init.
    /// Race flow leaves the car idle until <see cref="IRaceDriver.StartDriving"/> is invoked.
    /// </summary>
    public sealed class CarAutoStartDriver : IInitializable
    {
        private readonly IRaceDriver raceDriver;

        public CarAutoStartDriver(IRaceDriver raceDriver)
        {
            this.raceDriver = raceDriver;
        }

        public void Initialize()
        {
            raceDriver.StartDriving();
        }
    }
}
