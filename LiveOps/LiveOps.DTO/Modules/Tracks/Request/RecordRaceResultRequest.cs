using GameModuleDTO.GameApi;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class RecordRaceResultRequest : ModuleRequest<RecordRaceResultResponse>
    {
        public RecordRaceResultRequest()
        {
        }

        public RecordRaceResultRequest(string trackId, int score)
        {
            TrackId = trackId;
            Score = score;
        }

        [JsonProperty]
        public string TrackId { get; set; } = string.Empty;

        [JsonProperty]
        public int Score { get; set; }
    }
}
