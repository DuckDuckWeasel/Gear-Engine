using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(RecordRaceResultRequest))]
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
