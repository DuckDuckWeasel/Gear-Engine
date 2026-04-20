using GearEngine.CarSimulation;
using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    public sealed class RaceManagerService : ITickable
    {
        private readonly List<RaceState> activeRaces = new List<RaceState>();
        private readonly SplineCarRunnerService carRunner;

        public RaceManagerService(SplineCarRunnerService carRunner)
        {
            this.carRunner = carRunner ?? throw new ArgumentNullException(nameof(carRunner));
            // Subscribe to physical lap completed events from the simulation runner
            this.carRunner.OnLapCompleted += HandlePhysicalLapCompleted;
        }

        public void RegisterRace(RaceState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!activeRaces.Contains(state))
            {
                activeRaces.Add(state);
            }
        }

        public void UnregisterRace(RaceState state)
        {
            if (state != null)
            {
                activeRaces.Remove(state);
            }
        }

        public void StartRace(RaceState state)
        {
            if (state.Phase == SimulationLifecycleState.Created || state.Phase == SimulationLifecycleState.Paused)
            {
                state.Phase = SimulationLifecycleState.Running;
                state.PreviousLapStartTime = state.RaceTime;
                carRunner.SetPaused(state.Car, false);
            }
        }

        public void StopRace(RaceState state)
        {
            if (state.Phase == SimulationLifecycleState.Running)
            {
                state.Phase = SimulationLifecycleState.Paused;
                carRunner.SetPaused(state.Car, true);
            }
        }

        public void ForceFinish(RaceState state)
        {
            if (state.Phase != SimulationLifecycleState.Completed)
            {
                state.Phase = SimulationLifecycleState.Completed;
                carRunner.SetPaused(state.Car, true);
                Debug.Log($"[RaceManagerService] Race Finished! Total Laps: {state.TotalLaps} | Total Time: {state.RaceTime:F2}s");
                state.TriggerPresentationChanged();
            }
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            foreach (var state in activeRaces)
            {
                if (state.Phase == SimulationLifecycleState.Running)
                {
                    state.RaceTime += dt;
                }
            }
        }

        public RaceState GetFirstRaceForDebug()
        {
            return activeRaces.Count > 0 ? activeRaces[0] : null;
        }

        private void HandlePhysicalLapCompleted(CarEntity car)
        {
            // Find the RaceState linked to this physical car
            RaceState state = activeRaces.Find(r => r.Car == car);
            if (state == null || state.Phase != SimulationLifecycleState.Running) return;

            // Register the lap
            state.CurrentLap++;
            float lapTime = state.RaceTime - state.PreviousLapStartTime;
            state.AddLapTime(lapTime);
            state.PreviousLapStartTime = state.RaceTime;

            if (state.TotalLaps > 0 && state.CurrentLap <= state.TotalLaps)
            {
                Debug.Log($"[RaceManagerService] Lap {state.CurrentLap}/{state.TotalLaps} accomplished in {lapTime:F2}s");
            }

            // Check if finished
            if (state.TotalLaps > 0 && state.CurrentLap >= state.TotalLaps)
            {
                ForceFinish(state);
            }
        }
    }
}
