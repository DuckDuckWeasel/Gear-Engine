using System.Collections.Generic;
using GearEngine.Campaign;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public interface ITrackService
    {
        TrackDefinition CurrentTrack { get; }

        CarDefinition CurrentCar { get; }

        RaceState CurrentSession { get; }

        TrackProgressModel GetTrackProgress();

        IReadOnlyList<TrackEntry> GetOrderedTracks();

        IReadOnlyList<GearConfig> GetRoguelikeCardOptions();

        void SetCurrentSession(RaceState session);

        void RecordResult(RaceResultModel result);

        void AdvanceToNextTrack();
    }
}
