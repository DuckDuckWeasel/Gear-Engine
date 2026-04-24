using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class RecordRaceResultResponse : ModuleResponse
    {
        [JsonProperty]
        public float NewBestTimeSec { get; set; }

        [JsonProperty]
        public int MatchedBandIndex { get; set; } = -1;

        [JsonProperty]
        public int Reward { get; set; }

        [JsonProperty]
        public bool Advanced { get; set; }

        [JsonProperty]
        public string NextTrackId { get; set; } = string.Empty;
    }
}
