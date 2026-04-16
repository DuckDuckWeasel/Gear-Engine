using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.Cards.Powerups;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed partial class TrackSimulation : ViewModel
    {
        public TrackSimulation(TrackDefinition track, CarEntity car, CarPowerupBuildResult powerups = default)
        {
            this.track = track;
            this.car = car;
            this.powerups = powerups.MaxSpeedMultiplier > 0f && powerups.GripMultiplier > 0f ? powerups : CarPowerupBuildResult.Neutral;
        }

        public TrackDefinition Track => track;

        private readonly TrackDefinition track;

        public CarEntity Car => car;

        private readonly CarEntity car;

        public CarPowerupBuildResult Powerups => powerups;

        private readonly CarPowerupBuildResult powerups;

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
