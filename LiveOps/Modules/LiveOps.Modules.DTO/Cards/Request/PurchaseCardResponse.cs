using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class PurchaseCardResponse : ModuleResponse
    {
        [JsonProperty]
        public bool Success { get; set; }

        [JsonProperty]
        public string UnlockedCardId { get; set; } = string.Empty;

        [JsonProperty]
        public long NextCost { get; set; }

        [JsonProperty]
        public long Cost { get; set; }
    }
}
