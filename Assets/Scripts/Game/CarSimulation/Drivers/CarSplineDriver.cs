using System;
using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Splines;

namespace Scaffold.CarSimulation
{
    internal sealed class CarSplineDriver : MonoBehaviour
    {
        [SerializeField] private SplineAnimate splineAnimate;
        [SerializeField] private AttributeSO speedAttribute;

        private CarEntity car = null!;
        private IDisposable speedSubscription;

        public void Bind(CarEntity carEntity, SplineContainer splineContainer)
        {
            ValidateBindArguments(carEntity, splineContainer);
            speedSubscription?.Dispose();
            car = carEntity;
            ApplySplineSettings(splineContainer);
            speedSubscription = car.Instance.SubscribeToAttribute<FloatAttributeValue>(speedAttribute, OnSpeedChanged);
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

        private void OnSpeedChanged(FloatAttributeValue value)
        {
            if (splineAnimate != null)
            {
                splineAnimate.MaxSpeed = value.Value;
            }
        }

        private void OnDestroy()
        {
            speedSubscription?.Dispose();
        }
    }
}
