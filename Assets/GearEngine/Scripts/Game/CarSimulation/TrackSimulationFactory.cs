using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        private readonly CarEntityFactory carEntityFactory = new CarEntityFactory();

        public LapRaceSession Create(CarDefinition carDefinition, TrackDefinition trackDefinition, RaceSessionConfig config = null)
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
            RaceSessionConfig resolved = config ?? new RaceSessionConfig();
            return new LapRaceSession(trackDefinition, car, resolved);
        }
    }
}
