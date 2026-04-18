using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class RaceSessionRunner : IRaceSessionRunner, ITickable
    {
        public LapRaceSession ActiveSession => session;

        private LapRaceSession session;

        public void SetSession(LapRaceSession newSession)
        {
            session = newSession;
        }

        public void Tick()
        {
            session?.Tick(Time.deltaTime);
        }
    }
}
