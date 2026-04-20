using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalTrackService : ITrackService
    {
        public LocalTrackService(IReadOnlyList<TrackEntry> tracks, GearConfig[] roguelikeCardPool, TrackProgressModel trackProgress = null)
        {
            if (tracks == null || tracks.Count == 0)
            {
                throw new ArgumentException("[LocalTrackService] Track list must not be empty.", nameof(tracks));
            }

            this.tracks = tracks;
            this.roguelikeCardPool = roguelikeCardPool ?? Array.Empty<GearConfig>();
            this.trackProgress = trackProgress ?? new TrackProgressModel();
            this.trackProgress.CurrentTrackIndex = 0;
        }

        public TrackDefinition CurrentTrack => tracks[trackProgress.CurrentTrackIndex].Track;

        public CarDefinition CurrentCar => tracks[trackProgress.CurrentTrackIndex].Car;

        private readonly IReadOnlyList<TrackEntry> tracks;
        private readonly GearConfig[] roguelikeCardPool;
        private readonly TrackProgressModel trackProgress;

        public TrackProgressModel GetTrackProgress() => trackProgress;

        public IReadOnlyList<TrackEntry> GetOrderedTracks() => tracks;

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions()
        {
            return roguelikeCardPool;
        }

        public Task RecordResultAsync(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Debug.Log($"[LocalTrackService] Result recorded — Score: {result.Score}, Gold: {result.Gold.Amount} (stub, not persisted).");
            return Task.CompletedTask;
        }
    }
}
