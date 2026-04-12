using System;
using UnityEngine;

namespace Game.CarSimulation
{
    public sealed class TrackSimulationService : ITrackSimulationService
    {
        public TrackViewModel TrackViewModel { get; private set; }

        private SimulationLifecycleState lifecycleState = SimulationLifecycleState.None;

        public void CreateSimulation(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            try
            {
                CreateSimulationCore(carDefinition, trackDefinition);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationService] CreateSimulation failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void CreateSimulationCore(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            GuardCreateArguments(carDefinition, trackDefinition);
            if (TrackViewModel != null)
            {
                throw new InvalidOperationException("Simulation already created.");
            }

            CarEntity car = CreateCarEntityFromDefinition(carDefinition);
            TrackViewModel = new TrackViewModel(trackDefinition, car);
            lifecycleState = SimulationLifecycleState.Created;
            TrackViewModel.SetRunning(false);
        }

        public void ToggleSimulation(bool isRunning)
        {
            try
            {
                ToggleSimulationCore(isRunning);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationService] ToggleSimulation failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void ToggleSimulationCore(bool isRunning)
        {
            EnsureTrackViewModelExists("toggling");
            if (lifecycleState == SimulationLifecycleState.Completed)
            {
                throw new InvalidOperationException("Simulation has completed and cannot be toggled.");
            }

            if (isRunning)
            {
                TryStartOrResume();
            }
            else
            {
                TryPause();
            }
        }

        public void CompleteSimulation()
        {
            try
            {
                CompleteSimulationCore();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationService] CompleteSimulation failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void CompleteSimulationCore()
        {
            EnsureTrackViewModelExists("completing");
            if (lifecycleState != SimulationLifecycleState.Running && lifecycleState != SimulationLifecycleState.Paused)
            {
                throw new InvalidOperationException("Simulation can only be completed while running or paused.");
            }

            lifecycleState = SimulationLifecycleState.Completed;
            TrackViewModel.SetRunning(false);
        }

        private void GuardCreateArguments(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            if (trackDefinition == null)
            {
                throw new ArgumentNullException(nameof(trackDefinition));
            }
        }

        private void EnsureTrackViewModelExists(string operationHint)
        {
            if (TrackViewModel == null)
            {
                throw new InvalidOperationException($"Call CreateSimulation before {operationHint}.");
            }
        }

        private void TryStartOrResume()
        {
            if (lifecycleState == SimulationLifecycleState.Running)
            {
                return;
            }

            if (lifecycleState == SimulationLifecycleState.Created || lifecycleState == SimulationLifecycleState.Paused)
            {
                lifecycleState = SimulationLifecycleState.Running;
                TrackViewModel.SetRunning(true);
                return;
            }

            throw new InvalidOperationException("Simulation cannot be started from the current state.");
        }

        private void TryPause()
        {
            if (lifecycleState != SimulationLifecycleState.Running)
            {
                throw new InvalidOperationException("Simulation can only be paused while running.");
            }

            lifecycleState = SimulationLifecycleState.Paused;
            TrackViewModel.SetRunning(false);
        }

        private static CarEntity CreateCarEntityFromDefinition(CarDefinition definition)
        {
            return CarEntity.Create(definition);
        }
    }
}
