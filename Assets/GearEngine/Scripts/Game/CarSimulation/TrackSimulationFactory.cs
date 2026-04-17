using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        public TrackSimulationFactory()
        {

        }

        private readonly CarEntityFactory carEntityFactory = new CarEntityFactory();

        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            return Create(carDefinition, trackDefinition, null);
        }

        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition, TrackSimulationConfig config)
        {
            try
            {
                return CreateCore(carEntityFactory, carDefinition, trackDefinition, config);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TrackSimulationFactory] Create failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static TrackSimulation CreateCore(CarEntityFactory carEntityFactory, CarDefinition carDefinition, TrackDefinition trackDefinition, TrackSimulationConfig config)
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
            BakedTrackProfile profile = TrackProfileBaker.Bake(trackDefinition.Spline);
            CarVariableSet variables = config != null ? config.Variables : null;
            TrackSimulationTuning tuning = config != null ? config.Tuning : null;
            if (tuning == null)
            {
                tuning = ScriptableObject.CreateInstance<TrackSimulationTuning>();
            }

            return new TrackSimulation(trackDefinition, car, profile, variables, tuning);
        }
    }
}
