using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class RecordRaceResultResponse : ModuleResponse
    {
        [JsonProperty]
        public int NewBestScore { get; set; }

        [JsonProperty]
        public bool Advanced { get; set; }

        [JsonProperty]
        public string NextTrackId { get; set; } = string.Empty;
    }
}
