using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
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
            CarMotionState motion = simulation.Motion;
            motion.Distance = 0f;
            UpdateTransform(motion, simulation.Context.Profile);
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

            UpdateTransform(simulation.Motion, simulation.Context.Profile);
        }

        private void UpdateTransform(CarMotionState motion, BakedTrackProfile profile)
        {
            TrackSample s = profile.Evaluate(motion.Distance);
            Vector3 fwd = splineContainer.transform.TransformDirection(s.Forward).normalized;
            Vector3 up = splineContainer.transform.TransformDirection(s.Up).normalized;
            Vector3 right = Vector3.Cross(up, fwd).normalized;
            Vector3 worldPos = splineContainer.transform.TransformPoint(s.Position + right * motion.LateralOffset);
            Quaternion baseRot = Quaternion.LookRotation(fwd, up);
            transform.SetPositionAndRotation(worldPos, baseRot * Quaternion.Euler(0f, motion.SlipAngle, 0f));
        }
    }
}
