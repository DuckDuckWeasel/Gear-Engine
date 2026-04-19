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
            ScoreBands = BuildOrderedScoreBands(track);
        }

        public string TrackName { get; }

        public int TargetLaps { get; }

        public float TargetTime { get; }

        public IReadOnlyList<TrackScoreBandViewModel> ScoreBands { get; }

        protected override void Initialize()
        {
            base.Initialize();

            foreach (TrackScoreBandViewModel bandVm in ScoreBands)
            {
                BindChildViewModel(bandVm);
            }
        }

        private static List<TrackScoreBandViewModel> BuildOrderedScoreBands(TrackDefinition track)
        {
            if (!track.HasConfiguredScoreBands)
            {
                return new List<TrackScoreBandViewModel>();
            }

            return track.ScoreBands
                .Where(b => b != null)
                .OrderBy(b => b.MaxRaceTimeSeconds)
                .Select((band, idx) => new TrackScoreBandViewModel(idx + 1, band))
                .ToList();
        }
    }
}
