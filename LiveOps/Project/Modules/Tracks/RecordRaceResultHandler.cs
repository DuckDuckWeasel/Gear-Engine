using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Tracks;

namespace GameModule.Modules.Tracks
{
    public sealed class RecordRaceResultHandler : IGameApiHandler<RecordRaceResultRequest, RecordRaceResultResponse>
    {
        public async Task<RecordRaceResultResponse> HandleAsync(GameApiSession session, RecordRaceResultRequest request)
        {
            TrackConfig config = await session.RemoteConfig.Get(session.Context, TracksModule.ConfigKey, new TrackConfig()).ConfigureAwait(false);
            TrackPersistence persistence = await session.Player.Get(session.Context, TracksModule.PersistenceKey, new TrackPersistence()).ConfigureAwait(false);

            if (request == null || !config.TryGet(request.TrackId, out TrackConfigEntry entry) || entry == null)
            {
                return new RecordRaceResultResponse();
            }

            int previous = persistence.BestScores.TryGetValue(request.TrackId, out int v) ? v : 0;
            int best = System.Math.Max(previous, request.Score);
            persistence.BestScores[request.TrackId] = best;

            bool advanced = request.Score >= entry.AdvanceScore;
            string nextId = string.Empty;
            if (advanced)
            {
                List<TrackConfigEntry> list = config.Entries.ToList();
                int idx = list.FindIndex(e => e != null && e.Id == request.TrackId);
                if (idx >= 0 && idx + 1 < list.Count && list[idx + 1] != null)
                {
                    nextId = list[idx + 1].Id;
                    persistence.CurrentTrackId = nextId;
                }
            }

            return new RecordRaceResultResponse
            {
                NewBestScore = best,
                Advanced = advanced,
                NextTrackId = nextId,
            };
        }
    }
}
