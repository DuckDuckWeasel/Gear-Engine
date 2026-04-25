using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>Id lookup for track ScriptableObjects published from Addressables; replaces <c>TrackCatalogSO</c> at runtime.</summary>
    public sealed class TrackAssetIndex
    {
        private readonly Dictionary<string, TrackDefinition> byId;
        private readonly IReadOnlyList<TrackDefinition> ordered;

        public TrackAssetIndex(IReadOnlyList<TrackDefinition> tracks, CarDefinition defaultCar)
        {
            if (defaultCar == null)
            {
                throw new ArgumentNullException(nameof(defaultCar));
            }

            DefaultCar = defaultCar;
            ordered = tracks ?? Array.Empty<TrackDefinition>();
            byId = new Dictionary<string, TrackDefinition>(StringComparer.Ordinal);
            for (int i = 0; i < ordered.Count; i++)
            {
                TrackDefinition t = ordered[i];
                if (t == null)
                {
                    continue;
                }

                string id = t.name;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (!byId.ContainsKey(id))
                {
                    byId[id] = t;
                }
            }
        }

        public CarDefinition DefaultCar { get; }

        public IReadOnlyList<TrackDefinition> All => ordered;

        public TrackDefinition GetTrack(string trackId)
        {
            if (string.IsNullOrEmpty(trackId))
            {
                return null;
            }

            return byId.TryGetValue(trackId, out TrackDefinition t) ? t : null;
        }

        public string GetFirstResolvableTrackId()
        {
            for (int i = 0; i < ordered.Count; i++)
            {
                TrackDefinition t = ordered[i];
                if (t == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(t.name))
                {
                    return t.name;
                }
            }

            return null;
        }

        public IReadOnlyList<TrackEntry> OrderedEntries(IReadOnlyList<string> orderedTrackIds)
        {
            if (orderedTrackIds == null || orderedTrackIds.Count == 0)
            {
                return Array.Empty<TrackEntry>();
            }

            var list = new List<TrackEntry>(orderedTrackIds.Count);
            foreach (string id in orderedTrackIds)
            {
                if (!string.IsNullOrEmpty(id) && byId.TryGetValue(id, out TrackDefinition t))
                {
                    list.Add(new TrackEntry(t));
                }
            }

            return list;
        }
    }
}
