using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    internal static class CarRaceStats
    {
        private const float defaultMaxStraightWhenUnset = 30f;

        private const float defaultAccelerationWhenUnset = 10f;

        internal static float ReadHandling(CarEntity car, CarVariableSet vars, LapSimulationConfig lap)
        {
            if (car == null || vars == null || lap == null)
            {
                return 0.5f;
            }

            return Mathf.Clamp01(ReadVariable(car, vars.Handling) / Mathf.Max(1e-6f, lap.HandlingNormalizationScale));
        }

        internal static float ReadMaxStraightSpeed(CarEntity car, CarVariableSet vars)
        {
            if (car == null || vars == null)
            {
                return defaultMaxStraightWhenUnset;
            }

            return Mathf.Max(0f, ReadVariable(car, vars.MaxStraightSpeed));
        }

        internal static float ReadAcceleration(CarEntity car, CarVariableSet vars)
        {
            if (car == null || vars == null)
            {
                return defaultAccelerationWhenUnset;
            }

            return Mathf.Max(0f, ReadVariable(car, vars.Acceleration));
        }

        private static float ReadVariable(CarEntity car, Scaffold.Entities.VariableSO variable)
        {
            if (variable == null)
            {
                return 0f;
            }

            return car.GetValue<float>(variable);
        }
    }
}
