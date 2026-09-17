using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Campaign
{
    public sealed class RaceStandingsModel
    {
        public RaceStandingsModel(float raceTime, TrackDefinition track, float? previousRaceTime = null)
        {
            if (float.IsNaN(raceTime) || float.IsInfinity(raceTime) || raceTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raceTime));
            }

            List<RaceStandingEntry> rivals = BuildRivals(track);
            PlayerPosition = PositionFor(rivals, raceTime);
            PreviousPosition = previousRaceTime.HasValue ? PositionFor(rivals, previousRaceTime.Value) : 4;
            Player = new RaceStandingEntry("YOU", raceTime, true);
            rivals.Insert(PlayerPosition - 1, Player);
            Entries = rivals.AsReadOnly();
        }

        private RaceStandingsModel(TrackDefinition track)
        {
            List<RaceStandingEntry> rivals = BuildRivals(track);
            Player = new RaceStandingEntry("YOU", 0f, true, false);
            PlayerPosition = PreviousPosition = 4;
            rivals.Add(Player);
            Entries = rivals.AsReadOnly();
        }

        public static RaceStandingsModel FromBestTime(float? bestTime, TrackDefinition track)
        {
            return bestTime.HasValue ? new RaceStandingsModel(bestTime.Value, track, bestTime) : new RaceStandingsModel(track);
        }

        public IReadOnlyList<RaceStandingEntry> Entries { get; }
        public RaceStandingEntry Player { get; }
        public int PlayerPosition { get; }
        public int PreviousPosition { get; }
        public int VisibleRowCount => PlayerPosition <= 3 ? 3 : 4;

        private int PositionFor(IEnumerable<RaceStandingEntry> rivals, float time)
        {
            // A tie does not beat an existing rival; repeated runs keep the same order.
            return 1 + rivals.Count(rival => rival.TimeSeconds <= time);
        }

        private static List<RaceStandingEntry> BuildRivals(TrackDefinition track)
        {
            float target = track != null && track.TimeToBeatSeconds > 0f ? track.TimeToBeatSeconds : 60f;
            string[] names = { "NOVA", "AXEL", "BLAZE" };
            List<RaceStandingEntry> rivals = new List<RaceStandingEntry>();
            for (int i = 0; i < 3; i++)
            {
                RaceOpponentConfig configured = track?.Opponents != null && i < track.Opponents.Count ? track.Opponents[i] : null;
                float time = configured != null && configured.RaceTimeSeconds > 0f && !float.IsInfinity(configured.RaceTimeSeconds) ? configured.RaceTimeSeconds : target * (1f + i * 0.12f);
                string name = string.IsNullOrWhiteSpace(configured?.DisplayName) ? names[i] : configured.DisplayName;
                rivals.Add(new RaceStandingEntry(name, time));
            }
            return rivals.OrderBy(entry => entry.TimeSeconds).ToList();
        }
    }
}
