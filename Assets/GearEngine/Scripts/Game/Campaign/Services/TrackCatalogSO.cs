using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Resolves track ids (Remote Config / LiveOps) to scene <see cref="TrackDefinition"/> assets.
    /// A single <see cref="DefaultCar"/> is used for racing (not tied to a specific track row).
    /// </summary>
    [CreateAssetMenu(fileName = "TrackCatalog", menuName = "GearEngine/Campaign/Track Catalog")]
    public sealed class TrackCatalogSO : ScriptableObject
    {
        [SerializeField]
        private CarDefinition defaultCar;

        [SerializeField]
        private TrackEntry[] trackEntries = Array.Empty<TrackEntry>();

        private readonly Dictionary<string, TrackEntry> _byTrackId = new Dictionary<string, TrackEntry>(StringComparer.Ordinal);

        /// <summary>Car used for the active race session; configured once on this catalog.</summary>
        public CarDefinition DefaultCar => defaultCar;

        private void OnEnable()
        {
            RebuildLookup();
        }

        /// <summary>
        /// Replaces catalog data at runtime (e.g. from tests or dynamic content).
        /// </summary>
        public void SetRuntimeEntries(TrackEntry[] entries)
        {
            trackEntries = entries != null ? entries : Array.Empty<TrackEntry>();
            RebuildLookup();
        }

        /// <summary>
        /// Replaces the default car at runtime (e.g. from tests).
        /// </summary>
        public void SetRuntimeDefaultCar(CarDefinition car)
        {
            defaultCar = car;
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

        /// <summary>
        /// Authoritative ordered list of track entries (Remote Config ids align with <see cref="TrackEntry.TrackId"/>).
        /// </summary>
        public IReadOnlyList<TrackEntry> Entries => trackEntries ?? Array.Empty<TrackEntry>();

        public bool TryGetEntry(string trackId, out TrackEntry entry)
        {
            if (string.IsNullOrEmpty(trackId))
            {
                entry = null;
                return false;
            }

            return _byTrackId.TryGetValue(trackId, out entry);
        }

        public TrackDefinition GetTrack(string trackId)
        {
            if (string.IsNullOrEmpty(trackId) || !_byTrackId.TryGetValue(trackId, out TrackEntry entry))
            {
                return null;
            }

            return entry.Track;
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
    }
}
