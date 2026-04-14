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
        }

        public RaceStartData(TrackDefinition trackDefinition, CarDefinition carDefinition, GearEngineStartData gearEngineData = null, CarVariableSet carVariables = null)
        {
            this.trackDefinition = trackDefinition;
            this.carDefinition = carDefinition;
            this.gearEngineData = gearEngineData;
            this.carVariables = carVariables;
        }

        public TrackDefinition TrackDefinition => trackDefinition;

        public CarDefinition CarDefinition => carDefinition;

        public CarVariableSet CarVariables => carVariables;

        public GearEngineStartData GearEngineData => gearEngineData;

        [SerializeField]
        private TrackDefinition trackDefinition;

        [SerializeField]
        private CarDefinition carDefinition;

        [SerializeField]
        private CarVariableSet carVariables;

        [SerializeField]
        private GearEngineStartData gearEngineData;
    }
}
