using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Tracks;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        private readonly CarEntityFactory carEntityFactory = new CarEntityFactory();

        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition, TrackSimulationConfig config = null)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            if (trackDefinition == null)
            {
                throw new ArgumentNullException(nameof(trackDefinition));
            }

            return CreateCore(carDefinition, trackDefinition, config);
        }

        private TrackSimulation CreateCore(CarDefinition carDefinition, TrackDefinition trackDefinition, TrackSimulationConfig config)
        {
            CarEntity car = carEntityFactory.Create(carDefinition);
            CarVariableSet carVariables = config != null ? config.Variables : null;
            SimpleTrackDriverTuning tuning = config != null ? config.Driver : new SimpleTrackDriverTuning();
            SplineWaypointPath path = SplineWaypointPath.Build(trackDefinition.Spline, tuning.WaypointSpacingMetres);
            return new TrackSimulation(trackDefinition, car, path, carVariables, tuning);
        }
    }
}
