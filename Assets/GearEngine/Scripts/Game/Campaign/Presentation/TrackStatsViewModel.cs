using System;
using GearEngine.Campaign.Services;
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

            TrackName = trackService.CurrentTrack.name;
            TargetLaps = 3;
            TargetTime = 60f;
        }

        public string TrackName { get; }
        public int TargetLaps { get; }
        public float TargetTime { get; }
    }
}
