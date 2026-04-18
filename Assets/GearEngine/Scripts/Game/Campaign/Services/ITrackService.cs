using System.Collections.Generic;
using GearEngine.Campaign;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public interface ITrackService
    {
        TrackDefinition CurrentTrack { get; }

        CarDefinition CurrentCar { get; }

        TrackProgressModel GetTrackProgress();

        IReadOnlyList<TrackEntry> GetOrderedTracks();

        IReadOnlyList<GearConfig> GetRoguelikeCardOptions();

        void RecordResult(RaceResultModel result);

        void AdvanceToNextTrack();
    }
}
