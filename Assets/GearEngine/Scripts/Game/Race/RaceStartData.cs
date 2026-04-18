using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using UnityEngine;

namespace GearEngine.Race
{
    [Serializable]
    public sealed class RaceStartData
    {
        public RaceStartData()
        {
            simulationConfig = new TrackSimulationConfig();
        }

        public RaceStartData(TrackDefinition trackDefinition, CarDefinition carDefinition, GearEngineStartData gearEngineData = null, TrackSimulationConfig simulationConfig = null)
        {
            this.trackDefinition = trackDefinition;
            this.carDefinition = carDefinition;
            this.gearEngineData = gearEngineData;
            this.simulationConfig = simulationConfig ?? new TrackSimulationConfig();
        }

        public TrackDefinition TrackDefinition => trackDefinition;

        public CarDefinition CarDefinition => carDefinition;

        public TrackSimulationConfig SimulationConfig => simulationConfig;

        public GearEngineStartData GearEngineData => gearEngineData;

        [SerializeField]
        private TrackDefinition trackDefinition;

        [SerializeField]
        private CarDefinition carDefinition;

        [SerializeField]
        private TrackSimulationConfig simulationConfig = new TrackSimulationConfig();

        [SerializeField]
        private GearEngineStartData gearEngineData;
    }
}
