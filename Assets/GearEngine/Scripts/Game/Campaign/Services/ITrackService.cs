using System.Collections.Generic;
using System.Threading.Tasks;
using GearEngine.Campaign;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.Campaign.Services
{
    public interface ITrackService
    {
        TrackDefinition CurrentTrack { get; }

        CarDefinition CurrentCar { get; }

        TrackProgressModel GetTrackProgress();

        IReadOnlyList<TrackEntry> GetOrderedTracks();

        bool IsTrackUnlocked(string trackId);

        bool TrySelectTrack(string trackId);

        Task RecordResultAsync(RaceResultModel result);
    }
}
