using GearEngine.CarSimulation;
using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class RaceState
    {
        public CarEntity Car { get; }
        public TrackDefinition Track { get; }
        public int TotalLaps { get; }

        public SimulationLifecycleState Phase { get; set; } = SimulationLifecycleState.Created;
        public int CurrentLap { get; set; } = 0;
        public float RaceTime { get; set; } = 0f;
        public float PreviousLapStartTime { get; set; } = 0f;
        public float CurrentSpeed { get; set; } = 0f;
        public float NormalizedProgress { get; set; } = 0f;

        private readonly List<float> lapTimes = new List<float>();

        public IReadOnlyList<float> LapTimes => lapTimes;

        public RaceSessionConfig Config { get; }

        public event Action PresentationChanged;

        public RaceState(CarEntity car, TrackDefinition track, RaceSessionConfig config)
        {
            Car = car ?? throw new ArgumentNullException(nameof(car));
            Track = track ?? throw new ArgumentNullException(nameof(track));
            Config = config ?? new RaceSessionConfig();
            TotalLaps = Config.TotalLaps;
        }

        public void AddLapTime(float time)
        {
            lapTimes.Add(time);
        }

        public void TriggerPresentationChanged()
        {
            PresentationChanged?.Invoke();
        }

        public void Reset()
        {
            Phase = SimulationLifecycleState.Created;
            CurrentLap = 0;
            RaceTime = 0f;
            PreviousLapStartTime = 0f;
            lapTimes.Clear();
            TriggerPresentationChanged();
        }
    }
}
