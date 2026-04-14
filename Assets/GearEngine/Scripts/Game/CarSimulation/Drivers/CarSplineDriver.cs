using System;
using GearEngine.CarSimulation.Entity;
using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Drivers
{
    internal sealed class CarSplineDriver : MonoBehaviour
    {
        [SerializeField] private SplineAnimate splineAnimate;
        [FormerlySerializedAs("speedAttribute")]
        [SerializeField] private VariableSO speedVariable;

        private CarEntity car = null!;
        private IDisposable speedSubscription;

        public void Bind(CarEntity carEntity, SplineContainer splineContainer)
        {
            ValidateBindArguments(carEntity, splineContainer);
            speedSubscription?.Dispose();
            car = carEntity;
            ApplySplineSettings(splineContainer);
            speedSubscription = car.Instance.Subscribe(speedVariable, OnSpeedChanged);
        }

        private void ValidateBindArguments(CarEntity carEntity, SplineContainer splineContainer)
        {
            if (carEntity == null)
            {
                throw new ArgumentNullException(nameof(carEntity));
            }

            if (splineContainer == null)
            {
                throw new ArgumentNullException(nameof(splineContainer));
            }

            if (splineAnimate == null)
            {
                throw new InvalidOperationException("[CarSplineDriver] SplineAnimate reference is missing.");
            }
        }

        private void ApplySplineSettings(SplineContainer splineContainer)
        {
            splineAnimate.Container = splineContainer;
            splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
            splineAnimate.Easing = SplineAnimate.EasingMode.None;
            SnapToSplineStart();
        }

        private void SnapToSplineStart()
        {
            splineAnimate.NormalizedTime = 0f;
        }

        public void Play()
        {
            if (splineAnimate != null)
            {
                splineAnimate.Play();
            }
        }

        public void Stop()
        {
            if (splineAnimate != null)
            {
                splineAnimate.Pause();
            }
        }

        private void OnSpeedChanged(VariableValue value)
        {
            if (splineAnimate == null || value is not FloatVariableValue f)
            {
                return;
            }

            splineAnimate.MaxSpeed = f.Value;
        }

        private void OnDestroy()
        {
            speedSubscription?.Dispose();
        }
    }
}
