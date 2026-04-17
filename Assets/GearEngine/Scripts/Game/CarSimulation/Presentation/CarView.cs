using System;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarView : MonoBehaviour
    {
        [SerializeField] private SplineAnimate splineAnimate;

        private LapRaceSession session = null!;
        private SplineContainer splineContainer = null!;

        public void Initialize(CarEntity car, SplineContainer container, LapRaceSession lapSession)
        {
            ValidateInitializeArguments(car, container, lapSession);
            session = lapSession;
            splineContainer = container;
            splineAnimate = splineAnimate != null ? splineAnimate : GetComponent<SplineAnimate>();
            if (splineAnimate == null)
            {
                splineAnimate = gameObject.AddComponent<SplineAnimate>();
            }

            ApplySplineAnimateSettings();
            splineAnimate.Restart(false);
        }

        private void LateUpdate()
        {
            if (session == null || splineContainer == null)
            {
                return;
            }

            try
            {
                DriveSplineForSession();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CarView] LateUpdate failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DriveSplineForSession()
        {
            TryRestartSplineFromSession();
            PushSessionStateOntoSpline();
        }

        private void TryRestartSplineFromSession()
        {
            if (!session.ConsumePendingSplineRestart())
            {
                return;
            }

            ApplySplineAnimateSettings();
            splineAnimate.Restart(false);
        }

        private void PushSessionStateOntoSpline()
        {
            splineAnimate.MaxSpeed = Mathf.Max(0f, session.CurrentSpeed);
            splineAnimate.Pause();
            if (!session.IsSplineBound)
            {
                return;
            }

            float len = session.BoundTrackLength;
            splineAnimate.NormalizedTime = session.ProgressDistance / Mathf.Max(1e-4f, len);
        }

        private void ApplySplineAnimateSettings()
        {
            splineAnimate.Container = splineContainer;
            splineAnimate.PlayOnAwake = false;
            splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
            splineAnimate.Easing = SplineAnimate.EasingMode.None;
            splineAnimate.Loop = splineContainer.Spline.Closed ? SplineAnimate.LoopMode.Loop : SplineAnimate.LoopMode.Once;
        }

        private void ValidateInitializeArguments(CarEntity car, SplineContainer container, LapRaceSession lapSession)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (lapSession == null)
            {
                throw new ArgumentNullException(nameof(lapSession));
            }
        }
    }
}
