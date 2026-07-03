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
        private readonly ISimulationRunnerService runner;

        public RaceManagerService(ISimulationRunnerService runner)
        {
            this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
            // Subscribe to lap completed events from whichever runner is active
            this.runner.OnLapCompleted += HandlePhysicalLapCompleted;
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
                runner.SetPaused(state.Car, false);
            }
        }

        public void StopRace(RaceState state)
        {
            if (state.Phase == SimulationLifecycleState.Running)
            {
                state.Phase = SimulationLifecycleState.Paused;
                runner.SetPaused(state.Car, true);
            }
        }

        public void ForceFinish(RaceState state)
        {
            if (state.Phase != SimulationLifecycleState.Completed)
            {
                state.Phase = SimulationLifecycleState.Completed;
                runner.TriggerCinematicFinish(state.Car);
                runner.SetPaused(state.Car, true);
                Debug.Log($"[RaceManagerService] Race Finished! Total Laps: {state.TotalLaps} | Total Time: {state.RaceTime:F2}s");
                state.TriggerPresentationChanged();
            }
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            // Use a for loop since we might modify the collection indirectly, though foreach is mostly fine here
            for (int i = activeRaces.Count - 1; i >= 0; i--)
            {
                var state = activeRaces[i];
                if (state.Phase == SimulationLifecycleState.Running)
                {
                    state.RaceTime += dt;

                    // Anticipate the finish line for a cinematic slide (start slightly before the line)
                    if (state.TotalLaps > 0 && state.CurrentLap == state.TotalLaps - 1)
                    {
                        if (runner.GetTelemetry(state.Car, out CarTelemetryData telemetry))
                        {
                            // 0.95f is 5% before the finish line
                            if (telemetry.Progress >= 0.95f)
                            {
                                // Artificially complete the lap
                                state.CurrentLap++;
                                float lapTime = state.RaceTime - state.PreviousLapStartTime;
                                state.AddLapTime(lapTime);
                                state.PreviousLapStartTime = state.RaceTime;
                                
                                ForceFinish(state);
                            }
                        }
                    }
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
