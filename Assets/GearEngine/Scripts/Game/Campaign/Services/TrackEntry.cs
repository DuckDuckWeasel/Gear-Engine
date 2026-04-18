using System;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    [Serializable]
    public sealed class TrackEntry
    {
        public TrackDefinition Track => track;

        [SerializeField] private TrackDefinition track;

        public CarDefinition Car => car;

        [SerializeField] private CarDefinition car;
    }
}
