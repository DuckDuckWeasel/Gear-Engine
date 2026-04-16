using System;
using GearEngine.CarSimulation.Drivers;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarView : MonoBehaviour
    {
        [SerializeField] private CarSplineDriver splineDriver;

        public void Initialize(CarEntity car, SplineContainer splineContainer, LapRaceSession session)
        {
            GuardInitializeArguments(car, splineContainer, session);
            CarSplineDriver driver = ResolveSplineDriver();
            driver.Bind(session, splineContainer);
        }

        private void GuardInitializeArguments(CarEntity car, SplineContainer splineContainer, LapRaceSession session)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car));
            }

            if (splineContainer == null)
            {
                throw new ArgumentNullException(nameof(splineContainer));
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
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
    }
}
