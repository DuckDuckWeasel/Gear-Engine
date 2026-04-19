using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    /// <summary>Track spline preview for campaign screens that do not run race simulation.</summary>
    public sealed class CampaignTrackPreviewViewModel : ViewModel, ITrackDefinitionSource
    {
        public CampaignTrackPreviewViewModel(TrackDefinition track)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
        }

        public TrackDefinition Track { get; }
    }
}
