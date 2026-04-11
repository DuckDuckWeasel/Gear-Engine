using VContainer.Unity;

namespace Game.CarSimulation
{
    public sealed class CarAutoStartDriver : IInitializable
    {
        private readonly RaceFlowContracts.IRaceDriver raceDriver;

        public CarAutoStartDriver(RaceFlowContracts.IRaceDriver raceDriver)
        {
            this.raceDriver = raceDriver;
        }

        public void Initialize()
        {
            raceDriver.StartDriving();
        }
    }
}
