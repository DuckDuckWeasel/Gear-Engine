using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
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
