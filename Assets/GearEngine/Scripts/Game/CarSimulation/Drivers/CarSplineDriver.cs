using System;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Drivers
{
    internal sealed class CarSplineDriver : MonoBehaviour
    {
        private LapRaceSession session = null!;
        private SplineContainer splineContainer = null!;

        public void Bind(LapRaceSession lapRaceSession, SplineContainer container)
        {
            if (lapRaceSession == null)
            {
                throw new ArgumentNullException(nameof(lapRaceSession));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            session = lapRaceSession;
            splineContainer = container;
            ApplyPose(session.LastCurveSample, session.VisualState);
        }

        private void LateUpdate()
        {
            if (session == null || splineContainer == null || !session.IsCarPlaybackAllowed())
            {
                return;
            }

            ApplyPose(session.LastCurveSample, session.VisualState);
        }

        private void ApplyPose(CurveSample curve, CarVisualState visual)
        {
            Vector3 fwd = splineContainer.transform.TransformDirection(curve.Tangent).normalized;
            Vector3 up = splineContainer.transform.TransformDirection(curve.Up).normalized;
            Vector3 right = Vector3.Cross(up, fwd).normalized;
            Vector3 worldPos = splineContainer.transform.TransformPoint(curve.Position + right * visual.LateralOffset);
            Quaternion baseRot = Quaternion.LookRotation(fwd, up);
            transform.SetPositionAndRotation(worldPos, baseRot * Quaternion.Euler(0f, visual.SlipAngle, 0f));
        }
    }
}
