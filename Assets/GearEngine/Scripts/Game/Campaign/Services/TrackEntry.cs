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

        /// <summary>Stable id for LiveOps / Remote Config; defaults to the track asset name.</summary>
        public string TrackId => track != null ? track.name : string.Empty;
    }
}
