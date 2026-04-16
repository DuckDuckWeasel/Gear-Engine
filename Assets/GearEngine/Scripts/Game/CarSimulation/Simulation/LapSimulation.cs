using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class LapSimulation
    {
        public LapSimulation(RaceState state, LapSimulationConfig config)
        {
            this.state = state ?? throw new System.ArgumentNullException(nameof(state));
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
        }

        public RaceState State => state;

        private readonly RaceState state;
        private readonly LapSimulationConfig config;

        public void Tick(float dt, CarEntity car, CarVariableSet vars, CurveSample curve, float trackLength, bool isClosed)
        {
            if (dt <= 0f || trackLength < 1e-4f || state.Lifecycle != RaceLifecycle.Running)
            {
                return;
            }

            float handlingStat = CarRaceStats.ReadHandling(car, vars, config);
            float maxStraight = CarRaceStats.ReadMaxStraightSpeed(car, vars);
            float accel = CarRaceStats.ReadAcceleration(car, vars);
            float targetSpeed = maxStraight * (1f - curve.CurveAmount * (1f - handlingStat) * config.CurveSlowdown);
            state.CurrentSpeed = Mathf.MoveTowards(state.CurrentSpeed, targetSpeed, accel * dt);
            if (!isClosed)
            {
                TickOpenTrack(dt, trackLength);
                return;
            }

            AdvanceClosedTrack(dt, trackLength);
        }

        private void TickOpenTrack(float dt, float trackLength)
        {
            state.ProgressDistance += state.CurrentSpeed * dt;
            state.RaceTime += dt;
            if (state.ProgressDistance >= trackLength)
            {
                state.ProgressDistance = trackLength;
                state.CurrentSpeed = 0f;
                state.NormalizedProgress = 1f;
                state.Lifecycle = RaceLifecycle.Finished;
                return;
            }

            state.NormalizedProgress = state.ProgressDistance / trackLength;
        }

        private void AdvanceClosedTrack(float dt, float trackLength)
        {
            state.ProgressDistance += state.CurrentSpeed * dt;
            state.RaceTime += dt;
            state.NormalizedProgress = (state.ProgressDistance % trackLength) / trackLength;
            int nextLap = Mathf.FloorToInt(state.ProgressDistance / trackLength);
            if (nextLap > state.CurrentLap)
            {
                float lapTime = state.RaceTime - state.PreviousLapStartTime;
                state.LapTimes.Add(lapTime);
                state.PreviousLapStartTime = state.RaceTime;
            }

            state.CurrentLap = nextLap;
            if (config.TotalLaps >= 0 && state.CurrentLap >= config.TotalLaps)
            {
                state.Lifecycle = RaceLifecycle.Finished;
            }
        }
    }
}
