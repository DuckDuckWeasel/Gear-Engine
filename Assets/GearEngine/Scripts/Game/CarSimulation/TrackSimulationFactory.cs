using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        public TrackSimulationFactory() : this(new CarEntityFactory())
        {
        }

        public TrackSimulationFactory(CarEntityFactory carEntityFactory)
        {
            this.carEntityFactory = carEntityFactory ?? throw new ArgumentNullException(nameof(carEntityFactory));
        }

        private readonly CarEntityFactory carEntityFactory;

        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            try
            {
                return CreateCore(carEntityFactory, carDefinition, trackDefinition);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationFactory] Create failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static TrackSimulation CreateCore(CarEntityFactory carEntityFactory, CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            if (trackDefinition == null)
            {
                throw new ArgumentNullException(nameof(trackDefinition));
            }

            CarEntity car = carEntityFactory.Create(carDefinition);
            return new TrackSimulation(trackDefinition, car);
        }
    }
}
