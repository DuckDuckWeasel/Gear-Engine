using System;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            try
            {
                return CreateCore(carDefinition, trackDefinition);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationFactory] Create failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static TrackSimulation CreateCore(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            if (trackDefinition == null)
            {
                throw new ArgumentNullException(nameof(trackDefinition));
            }

            CarEntity car = CarEntity.Create(carDefinition);
            return new TrackSimulation(trackDefinition, car);
        }
    }
}
