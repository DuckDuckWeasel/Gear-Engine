using GearEngine.CarSimulation;

namespace GearEngine.CarSimulation.Simulation
{
    public interface IRaceSessionRunner
    {
        LapRaceSession ActiveSession { get; }

        void SetSession(LapRaceSession session);

        void Tick();
    }
}
