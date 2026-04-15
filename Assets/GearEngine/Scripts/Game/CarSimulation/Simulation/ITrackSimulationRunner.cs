using GearEngine.CarSimulation;

namespace GearEngine.CarSimulation.Simulation
{
    public interface ITrackSimulationRunner
    {
        void SetSimulation(TrackSimulation simulation);

        void Tick();
    }
}
