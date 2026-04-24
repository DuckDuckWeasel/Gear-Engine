using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("RecordRaceResultRequest")]
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
