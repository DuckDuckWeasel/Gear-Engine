using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulation : Model
    {
        public TrackSimulation(
            TrackDefinition track,
            CarEntity car,
            SplineWaypointPath waypointPath,
            CarVariableSet carVariables,
            SimpleTrackDriverTuning driverTuning)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (waypointPath == null)
            {
                throw new ArgumentNullException(nameof(waypointPath));
            }

            Track = track;
            Car = car;
            WaypointPath = waypointPath;
            Variables = carVariables;
            DriverTuning = driverTuning ?? new SimpleTrackDriverTuning();
            Race = new RaceRuntimeState();
            Motion = new CarMotionState();
        }

        internal Transform TrackRootTransform { get; private set; }

        public TrackDefinition Track { get; }

        public CarEntity Car { get; }

        internal SplineWaypointPath WaypointPath { get; }

        internal CarVariableSet Variables { get; }

        internal SimpleTrackDriverTuning DriverTuning { get; }

        public RaceRuntimeState Race { get; }

        internal CarMotionState Motion { get; }

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
            SeedMotionFromTrack();
        }

        internal void AttachTrackRoot(Transform root)
        {
            TrackRootTransform = root;
        }

        internal void SeedMotionFromTrack()
        {
            if (TrackRootTransform == null || WaypointPath == null)
            {
                return;
            }

            SimpleWaypointDriver.SeedStart(Motion, WaypointPath, TrackRootTransform);
        }
    }
}
