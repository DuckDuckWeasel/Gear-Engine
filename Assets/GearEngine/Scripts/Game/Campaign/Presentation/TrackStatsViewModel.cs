using System;
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
        }

        public string TrackName { get; }
        public int TargetLaps { get; }
        public float TargetTime { get; }
    }
}
