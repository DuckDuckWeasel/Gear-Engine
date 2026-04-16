using System.Collections.Generic;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class RaceState
    {
        public float ProgressDistance;
        public float NormalizedProgress;
        public float CurrentSpeed;
        public float RaceTime;
        public int CurrentLap;
        public readonly List<float> LapTimes = new List<float>();
        public float PreviousLapStartTime;
        public RaceLifecycle Lifecycle;

        public void Reset()
        {
            ProgressDistance = 0f;
            NormalizedProgress = 0f;
            CurrentSpeed = 0f;
            RaceTime = 0f;
            CurrentLap = 0;
            LapTimes.Clear();
            PreviousLapStartTime = 0f;
            Lifecycle = RaceLifecycle.Idle;
        }
    }
}
