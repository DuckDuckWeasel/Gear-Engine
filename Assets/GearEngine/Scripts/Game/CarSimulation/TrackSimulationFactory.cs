using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using VContainer.Unity;

namespace GearEngine.CarSimulation
{
    public sealed class TrackSimulationFactory
    {
        private readonly CarEntityFactory carEntityFactory = new CarEntityFactory();
        private readonly VContainer.IObjectResolver resolver;

        public TrackSimulationFactory(VContainer.IObjectResolver resolver = null)
        {
            this.resolver = resolver;
        }

        public RaceState Create(CarDefinition carDefinition, TrackDefinition trackDefinition, RaceSessionConfig config = null)
        {
            if (carDefinition == null) throw new ArgumentNullException(nameof(carDefinition));
            if (trackDefinition == null) throw new ArgumentNullException(nameof(trackDefinition));

            CarEntity car = carEntityFactory.Create(carDefinition);
            RaceSessionConfig resolved = config ?? new RaceSessionConfig();
            return new RaceState(car, trackDefinition, resolved);
        }
        
        public GameObject InstantiateCar(RaceState session, Transform parent)
        {
            GameObject prefab = session.Car.Definition.CarPrefab;
            if (prefab == null) return null;

            if (resolver != null) {
                return resolver.Instantiate(prefab, parent);
            } else {
                return UnityEngine.Object.Instantiate(prefab, parent);
            }
        }
    }
}
