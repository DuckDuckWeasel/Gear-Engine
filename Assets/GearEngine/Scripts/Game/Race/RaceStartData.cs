using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.Race
{
    [Serializable]
    public sealed class RaceStartData
    {
        public RaceStartData()
        {
            sessionConfig = new RaceSessionConfig();
        }

        public RaceStartData(TrackDefinition trackDefinition, CarDefinition carDefinition, GearEngineStartData gearEngineData = null, RaceSessionConfig sessionConfig = null)
        {
            this.trackDefinition = trackDefinition;
            this.carDefinition = carDefinition;
            this.gearEngineData = gearEngineData;
            this.sessionConfig = sessionConfig ?? new RaceSessionConfig();
        }

        public TrackDefinition TrackDefinition => trackDefinition;

        public CarDefinition CarDefinition => carDefinition;

        public RaceSessionConfig SessionConfig => sessionConfig;

        public GearEngineStartData GearEngineData => gearEngineData;

        [SerializeField] private TrackDefinition trackDefinition;
        [SerializeField] private CarDefinition carDefinition;
        [FormerlySerializedAs("simulationConfig")]
        [SerializeField] private RaceSessionConfig sessionConfig = new RaceSessionConfig();
        [SerializeField] private GearEngineStartData gearEngineData;
    }
}
