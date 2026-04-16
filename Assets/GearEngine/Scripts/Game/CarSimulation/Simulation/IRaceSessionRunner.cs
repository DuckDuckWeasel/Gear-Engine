using GearEngine.CarSimulation;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    public interface IRaceSessionRunner : ITickable
    {
        LapRaceSession ActiveSession { get; }

        void SetSession(LapRaceSession session);
    }
}
