using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Track;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed partial class TrackSimulation : Model
    {
        public TrackSimulation(TrackDefinition track, CarEntity car, BakedTrackProfile profile, CarVariableSet carVariables)
        {
            this.track = track ?? throw new ArgumentNullException(nameof(track));
            this.car = car ?? throw new ArgumentNullException(nameof(car));
            BakedProfile = profile ?? throw new ArgumentNullException(nameof(profile));
            CarVariables = carVariables;
            Race = new RaceRuntimeState();
        }

        public TrackDefinition Track => track;

        private readonly TrackDefinition track;

        public CarEntity Car => car;

        private readonly CarEntity car;

        public BakedTrackProfile BakedProfile { get; }

        public CarVariableSet CarVariables { get; }

        public RaceRuntimeState Race { get; }

        internal CarMotionState Motion { get; } = new CarMotionState();

        [ObservableProperty]
        private SimulationLifecycleState state = SimulationLifecycleState.Created;

        public void Toggle(bool running)
        {
            try
            {
                ToggleCore(running);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulation] Toggle failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void Complete()
        {
            try
            {
                CompleteCore();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulation] Complete failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void ToggleCore(bool running)
        {
            if (State == SimulationLifecycleState.Completed)
            {
                throw new InvalidOperationException("Simulation has completed and cannot be toggled.");
            }

            if (running)
            {
                TryStartOrResume();
            }
            else
            {
                TryPause();
            }
        }

        private void CompleteCore()
        {
            if (State != SimulationLifecycleState.Running && State != SimulationLifecycleState.Paused)
            {
                throw new InvalidOperationException("Simulation can only be completed while running or paused.");
            }

            State = SimulationLifecycleState.Completed;
        }

        private void TryStartOrResume()
        {
            if (State == SimulationLifecycleState.Running)
            {
                return;
            }

            if (State == SimulationLifecycleState.Created || State == SimulationLifecycleState.Paused)
            {
                if (State == SimulationLifecycleState.Created)
                {
                    ResetRuntimeState();
                }

                State = SimulationLifecycleState.Running;
                return;
            }

            throw new InvalidOperationException("Simulation cannot be started from the current state.");
        }

        private void TryPause()
        {
            if (State != SimulationLifecycleState.Running)
            {
                throw new InvalidOperationException("Simulation can only be paused while running.");
            }

            State = SimulationLifecycleState.Paused;
        }

        private void ResetRuntimeState()
        {
            Motion.Reset();
            Race.Reset();
        }
    }
}
