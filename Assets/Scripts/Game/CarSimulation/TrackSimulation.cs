using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Scaffold.MVVM;
using UnityEngine;

namespace Scaffold.CarSimulation
{
    public sealed partial class TrackSimulation : ViewModel
    {
        public TrackSimulation(TrackDefinition track, CarEntity car)
        {
            this.track = track;
            this.car = car;
        }

        public TrackDefinition Track => track;

        private readonly TrackDefinition track;

        public CarEntity Car => car;

        private readonly CarEntity car;

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
    }
}
