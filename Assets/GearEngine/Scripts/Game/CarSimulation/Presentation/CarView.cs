using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Drivers;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarView : MonoBehaviour
    {
        [SerializeField] private CarSplineDriver splineDriver;

        public void Initialize(CarEntity car, SplineContainer splineContainer, TrackSimulation trackSimulation)
        {
            GuardInitializeArguments(car, splineContainer, trackSimulation);
            CarSplineDriver driver = ResolveSplineDriver();
            driver.Bind(car, splineContainer);
        }

        private void GuardInitializeArguments(CarEntity car, SplineContainer splineContainer, TrackSimulation trackSimulation)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (splineContainer == null)
            {
                throw new ArgumentNullException(nameof(splineContainer));
            }

            if (trackSimulation == null)
            {
                throw new ArgumentNullException(nameof(trackSimulation));
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

        internal void OnRunningChanged(SimulationLifecycleState state)
        {
            if (splineDriver == null)
            {
                return;
            }

            if (state is SimulationLifecycleState.Running)
            {
                splineDriver.Play();
            }
            else
            {
                splineDriver.Stop();
            }
        }
    }
}
