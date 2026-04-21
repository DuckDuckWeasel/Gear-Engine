using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    public sealed class SaveBoardLayoutResponse : ModuleResponse
    {
        [JsonProperty]
        public long SavedAtUtcTicks { get; set; }

        [JsonProperty("rejected")]
        public bool Rejected { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
