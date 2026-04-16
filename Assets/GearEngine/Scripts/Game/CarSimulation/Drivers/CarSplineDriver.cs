using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Drivers
{
    internal sealed class CarSplineDriver : MonoBehaviour
    {
        private TrackSimulation simulation = null!;
        private SplineContainer splineContainer = null!;
        private bool playbackActive;

        public void Bind(TrackSimulation trackSimulation, SplineContainer container)
        {
            GuardBindArguments(trackSimulation, container);
            simulation = trackSimulation;
            splineContainer = container;
            playbackActive = false;
            simulation.AttachTrackRoot(container.transform);
            ApplyInitialPose();
        }

        private void GuardBindArguments(TrackSimulation trackSimulation, SplineContainer container)
        {
            if (trackSimulation == null)
            {
                throw new ArgumentNullException(nameof(trackSimulation));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
        }

        private void ApplyInitialPose()
        {
            simulation.SeedMotionFromTrack();
            UpdateTransform(simulation.Motion);
        }

        public void Play()
        {
            playbackActive = true;
        }

        public void Stop()
        {
            playbackActive = false;
        }

        private void Update()
        {
            if (!playbackActive || simulation == null || splineContainer == null)
            {
                return;
            }

            if (simulation.State != SimulationLifecycleState.Running)
            {
                return;
            }

            UpdateTransform(simulation.Motion);
        }

        private void UpdateTransform(CarMotionState motion)
        {
            Quaternion baseRot = Quaternion.Euler(0f, motion.YawDegrees, 0f);
            transform.SetPositionAndRotation(motion.Position, baseRot * Quaternion.Euler(0f, motion.SlipAngle, 0f));
        }
    }
}
