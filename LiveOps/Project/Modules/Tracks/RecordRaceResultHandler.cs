using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.Modules.Tracks;

namespace GameModule.Modules.Tracks
{
    public sealed class RecordRaceResultHandler : IGameApiHandler<RecordRaceResultRequest, RecordRaceResultResponse>
    {
        public async Task<RecordRaceResultResponse> HandleAsync(GameApiSession session, RecordRaceResultRequest request)
        {
            TrackConfig config = await session.RemoteConfig.Get(session.Context, TracksModule.ConfigKey, new TrackConfig());
            TrackPersistence persistence = await session.Player.Get(session.Context, TracksModule.PersistenceKey, new TrackPersistence());

            if (request == null || !config.TryGet(request.TrackId, out TrackConfigEntry entry) || entry == null)
            {
                return new RecordRaceResultResponse();
            }

            RecordRaceResultResponse response = TrackRecordRaceEvaluator.Evaluate(entry, config, request.TrackId, request.RaceTimeSec, persistence);

            if (response.Reward > 0)
            {
                await session.InvokeAsync<AddCurrencyRequest, AddCurrencyResponse>(new AddCurrencyRequest("gold", response.Reward));
            }

            return response;
        }
    }
}
