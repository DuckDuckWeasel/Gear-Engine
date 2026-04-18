using System.Collections.Generic;
using GearEngine.Campaign;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public interface ITrackService
    {
        TrackDefinition CurrentTrack { get; }
        CarDefinition CurrentCar { get; }
        LapRaceSession CurrentSession { get; }

        IReadOnlyList<GearConfig> GetRoguelikeCardOptions();

        void SetCurrentSession(LapRaceSession session);
        void RecordResult(RaceResultModel result);
        void AdvanceToNextTrack();
    }
}
