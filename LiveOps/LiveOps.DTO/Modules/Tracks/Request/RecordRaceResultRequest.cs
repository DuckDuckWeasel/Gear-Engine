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

        public RecordRaceResultRequest(string trackId, float raceTimeSec)
        {
            TrackId = trackId;
            RaceTimeSec = raceTimeSec;
        }

        [JsonProperty]
        public string TrackId { get; set; } = string.Empty;

        [JsonProperty]
        public float RaceTimeSec { get; set; }
    }
}
