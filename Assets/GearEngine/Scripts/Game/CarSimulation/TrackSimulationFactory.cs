using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Track;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        private readonly CarEntityFactory carEntityFactory = new CarEntityFactory();

        public TrackSimulation Create(CarDefinition carDefinition, TrackDefinition trackDefinition, CarVariableSet carVariables)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            if (trackDefinition == null)
            {
                throw new ArgumentNullException(nameof(trackDefinition));
            }

            return CreateCore(carDefinition, trackDefinition, carVariables);
        }

        private TrackSimulation CreateCore(CarDefinition carDefinition, TrackDefinition trackDefinition, CarVariableSet carVariables)
        {
            CarEntity car = carEntityFactory.Create(carDefinition);
            BakedTrackProfile profile = TrackProfileBaker.Bake(trackDefinition.Spline);
            return new TrackSimulation(trackDefinition, car, profile, carVariables);
        }
    }
}
