using System;
using System.Collections.Generic;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalTrackService : ITrackService
    {
        public LocalTrackService(IReadOnlyList<TrackEntry> tracks, GearConfig[] roguelikeCardPool)
        {
            if (tracks == null || tracks.Count == 0)
            {
                throw new ArgumentException("[LocalTrackService] Track list must not be empty.", nameof(tracks));
            }

            this.tracks = tracks;
            this.roguelikeCardPool = roguelikeCardPool ?? Array.Empty<GearConfig>();
            currentIndex = 0;
        }

        public TrackDefinition CurrentTrack => tracks[currentIndex].Track;
        public CarDefinition CurrentCar => tracks[currentIndex].Car;
        public LapRaceSession CurrentSession { get; private set; }

        private readonly IReadOnlyList<TrackEntry> tracks;
        private readonly GearConfig[] roguelikeCardPool;
        private int currentIndex;

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions()
        {
            return roguelikeCardPool;
        }

        public void SetCurrentSession(LapRaceSession session)
        {
            CurrentSession = session ?? throw new ArgumentNullException(nameof(session));
        }

        public void RecordResult(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Debug.Log($"[LocalTrackService] Result recorded — Score: {result.Score}, Gold: {result.Gold.Amount} (stub, not persisted).");
        }

        public void AdvanceToNextTrack()
        {
            if (currentIndex < tracks.Count - 1)
            {
                currentIndex++;
                Debug.Log($"[LocalTrackService] Advanced to track {currentIndex}: {CurrentTrack.name}");
            }
            else
            {
                Debug.Log("[LocalTrackService] Already on the last track — no advancement (stub).");
            }
        }
    }
}
