using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Resolves track ids (Remote Config / LiveOps) to scene <see cref="TrackDefinition"/> and <see cref="CarDefinition"/> assets.
    /// </summary>
    [CreateAssetMenu(fileName = "TrackCatalog", menuName = "GearEngine/Campaign/Track Catalog")]
    public sealed class TrackCatalogSO : ScriptableObject
    {
        [SerializeField]
        private TrackEntry[] trackEntries = Array.Empty<TrackEntry>();

        [SerializeField]
        private GearConfig[] roguelikeCardPool = Array.Empty<GearConfig>();

        private readonly Dictionary<string, TrackEntry> _byTrackId = new Dictionary<string, TrackEntry>(StringComparer.Ordinal);

        private void OnEnable()
        {
            RebuildLookup();
        }

        /// <summary>
        /// Replaces catalog data at runtime (e.g. from tests or dynamic content).
        /// </summary>
        public void SetRuntimeEntries(TrackEntry[] entries, GearConfig[] roguelikeCards)
        {
            trackEntries = entries != null ? entries : Array.Empty<TrackEntry>();
            roguelikeCardPool = roguelikeCards != null ? roguelikeCards : Array.Empty<GearConfig>();
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            _byTrackId.Clear();
            if (trackEntries == null)
            {
                return;
            }

            foreach (TrackEntry entry in trackEntries)
            {
                if (entry?.Track == null)
                {
                    continue;
                }

                string id = entry.TrackId;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                _byTrackId[id] = entry;
            }
        }

        /// <summary>
        /// First stable track id present in the catalog, or null when the catalog has no usable entries.
        /// </summary>
        public string GetFirstResolvableTrackId()
        {
            if (trackEntries == null)
            {
                return null;
            }

            foreach (TrackEntry entry in trackEntries)
            {
                if (entry?.Track == null)
                {
                    continue;
                }

                string id = entry.TrackId;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                return id;
            }

            return null;
        }

        public TrackDefinition GetTrack(string trackId)
        {
            if (string.IsNullOrEmpty(trackId) || !_byTrackId.TryGetValue(trackId, out TrackEntry entry))
            {
                return null;
            }

            return entry.Track;
        }

        public CarDefinition GetCarFor(string trackId)
        {
            if (string.IsNullOrEmpty(trackId) || !_byTrackId.TryGetValue(trackId, out TrackEntry entry))
            {
                return null;
            }

            return entry.Car;
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
                if (!string.IsNullOrEmpty(id) && _byTrackId.TryGetValue(id, out TrackEntry entry))
                {
                    list.Add(entry);
                }
            }

            return list;
        }

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions()
        {
            return roguelikeCardPool ?? Array.Empty<GearConfig>();
        }
    }
}
