using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackStatsViewModel : ViewModel
    {
        public TrackStatsViewModel(ITrackService trackService)
            : this(trackService?.CurrentTrack ?? throw new ArgumentNullException(nameof(trackService)), trackService.GetTrackProgress())
        {
        }

        public TrackStatsViewModel(TrackDefinition track, TrackProgressModel progress = null)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            Standings = RaceStandingsModel.FromBestTime(progress?.GetBestTimeSeconds(track.name), track);
            EarnedStars = progress?.GetEarnedStars(track.name) ?? 0;
            TrackName = track.GetDisplayName();
            TargetLaps = track.TotalLaps;
            TargetTime = track.TimeToBeatSeconds;
            Tiers = BuildOrderedTiers(track);
            StarTargetScores = Tiers.Select(tier => tier.TargetScore).ToArray();
        }

        public RaceStandingsModel Standings { get; }
        public int EarnedStars { get; }

        public string TrackName { get; }

        public int TargetLaps { get; }

        public float TargetTime { get; }

        public IReadOnlyList<TrackTierViewModel> Tiers { get; }

        public IReadOnlyList<int> StarTargetScores { get; }

        protected override void Initialize()
        {
            base.Initialize();

            foreach (TrackTierViewModel tierVm in Tiers)
            {
                BindChildViewModel(tierVm);
            }
        }

        private static List<TrackTierViewModel> BuildOrderedTiers(TrackDefinition track)
        {
            if (!track.HasConfiguredTiers)
            {
                return new List<TrackTierViewModel>();
            }

            return track.Tiers
                .Where(t => t != null)
                .OrderBy(t => t.TargetScore)
                .Select((tier, idx) => new TrackTierViewModel(idx + 1, tier))
                .ToList();
        }
    }
}
