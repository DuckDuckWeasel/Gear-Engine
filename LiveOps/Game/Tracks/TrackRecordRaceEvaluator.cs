using System;
using System.Collections.Generic;
using System.Linq;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Tracks;

namespace LiveOps.Modules.Tracks
{
    public static class TrackRecordRaceEvaluator
    {
        public static RecordRaceResultResponse Evaluate(
            TrackConfigEntry entry,
            TrackConfig config,
            string trackId,
            float raceTimeSec,
            TrackPersistence persistence)
        {
            if (entry == null || persistence == null || string.IsNullOrEmpty(trackId))
            {
                return new RecordRaceResultResponse();
            }

            if (raceTimeSec < 0f || float.IsNaN(raceTimeSec) || float.IsInfinity(raceTimeSec))
            {
                return new RecordRaceResultResponse();
            }

            List<TrackScoreBandConfig> ordered = (entry.Bands ?? Enumerable.Empty<TrackScoreBandConfig>())
                .Where(b => b != null && b.MaxRaceTimeSeconds > 0f)
                .OrderBy(b => b.MaxRaceTimeSeconds)
                .ToList();

            int matchedBandIndex = -1;
            int reward = entry.BaseReward;
            for (int i = 0; i < ordered.Count; i++)
            {
                if (raceTimeSec <= ordered[i].MaxRaceTimeSeconds)
                {
                    matchedBandIndex = i;
                    reward = ordered[i].Reward;
                    break;
                }
            }

            bool advanced = matchedBandIndex >= 0;

            float previousBest = persistence.BestTimeSec.TryGetValue(trackId, out float v) ? v : float.PositiveInfinity;
            float best = Math.Min(previousBest, raceTimeSec);
            persistence.BestTimeSec[trackId] = best;

            string nextId = string.Empty;
            if (advanced)
            {
                List<TrackConfigEntry> list = config.Entries.ToList();
                int idx = list.FindIndex(e => e != null && e.Id == trackId);
                if (idx >= 0 && idx + 1 < list.Count && list[idx + 1] != null)
                {
                    nextId = list[idx + 1].Id;
                    persistence.CurrentTrackId = nextId;
                }
            }

            return new RecordRaceResultResponse
            {
                NewBestTimeSec = best,
                MatchedBandIndex = matchedBandIndex,
                Reward = reward,
                Advanced = advanced,
                NextTrackId = nextId,
            };
        }
    }
}
