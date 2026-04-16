using GearEngine.CarSimulation;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class TrackSimulationRunner : ITickable, ITrackSimulationRunner
    {
        private TrackSimulation simulation;

        public TrackSimulation ActiveSimulation { get; private set; }

        public void SetSimulation(TrackSimulation sim)
        {
            simulation = sim;
            ActiveSimulation = sim;
        }

        public void Tick()
        {
            if (simulation?.State != SimulationLifecycleState.Running)
            {
                return;
            }

            Step(Time.deltaTime);
        }

        internal void Step(float dt)
        {
            if (simulation == null || simulation.State != SimulationLifecycleState.Running || dt <= 0f)
            {
                return;
            }

            StepCore(dt);
        }

        private void StepCore(float dt)
        {
            TrackSimulation sim = simulation;
            if (sim.WaypointPath == null || sim.WaypointPath.TotalLength < 1e-4f)
            {
                return;
            }

            CarMotionState motion = sim.Motion;
            motion.Speed += motion.PendingSpeedBoost;
            motion.PendingSpeedBoost = 0f;

            Transform trackTransform = sim.TrackRootTransform;
            if (trackTransform == null)
            {
                return;
            }

            SimpleWaypointDriver.Step(dt, sim, trackTransform, sim.DriverTuning);
        }
    }
}
