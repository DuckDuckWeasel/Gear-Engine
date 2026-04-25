using System;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// One catalog row: a <see cref="TrackDefinition"/> only. The race car comes from the injected <see cref="CarDefinition"/> in FoundationLayer (e.g. <c>GearAppFlowRoot.defaultRaceCar</c>), not per row.
    /// </summary>
    [Serializable]
    public sealed class TrackEntry
    {
        [SerializeField]
        private TrackDefinition track;

        public TrackEntry()
        {
        }

        public TrackEntry(TrackDefinition trackDefinition)
        {
            track = trackDefinition;
        }

        public TrackDefinition Track => track;

        /// <summary>Stable id for LiveOps / Remote Config; defaults to the track asset name.</summary>
        public string TrackId => track != null ? track.name : string.Empty;
    }
}
