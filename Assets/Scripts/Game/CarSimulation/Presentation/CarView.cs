using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation
{
    public sealed class CarView : MonoBehaviour
    {
        [SerializeField] private CarSplineDriver splineDriver;

        public void Initialize(CarEntity car, SplineContainer splineContainer, TrackViewModel trackViewModel)
        {
            GuardInitializeArguments(car, splineContainer, trackViewModel);
            CarSplineDriver driver = ResolveSplineDriver();
            driver.Bind(car, splineContainer);
            if (trackViewModel.IsRunning)
            {
                driver.Play();
            }
        }

        internal void OnRunningChanged(bool isRunning)
        {
            if (splineDriver == null)
            {
                return;
            }

            if (isRunning)
            {
                splineDriver.Play();
            }
            else
            {
                splineDriver.Stop();
            }
        }

        private CarSplineDriver ResolveSplineDriver()
        {
            if (splineDriver != null)
            {
                return splineDriver;
            }

            splineDriver = GetComponent<CarSplineDriver>();
            if (splineDriver == null)
            {
                throw new InvalidOperationException("[CarView] CarSplineDriver is missing on prefab.");
            }

            return splineDriver;
        }

        private void GuardInitializeArguments(CarEntity car, SplineContainer splineContainer, TrackViewModel trackViewModel)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (splineContainer == null)
            {
                throw new ArgumentNullException(nameof(splineContainer));
            }

            if (trackViewModel == null)
            {
                throw new ArgumentNullException(nameof(trackViewModel));
            }
        }
    }
}
