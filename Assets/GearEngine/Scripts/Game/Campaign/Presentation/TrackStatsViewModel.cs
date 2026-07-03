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
        {
            if (trackService == null)
            {
                throw new ArgumentNullException(nameof(trackService));
            }

            TrackDefinition track = trackService.CurrentTrack;
            TrackName = track.GetDisplayName();
            TargetLaps = track.TotalLaps;
            TargetTime = track.TimeToBeatSeconds;
            Tiers = BuildOrderedTiers(track);
        }

        public string TrackName { get; }

        public int TargetLaps { get; }

        public float TargetTime { get; }

        public IReadOnlyList<TrackTierViewModel> Tiers { get; }

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
                .Select((tier, idx) => new TrackTierViewModel(idx + 1, tier))
                .ToList();
        }
    }
}
