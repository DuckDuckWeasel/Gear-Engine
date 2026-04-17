using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class RaceSessionRunner : IRaceSessionRunner
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
