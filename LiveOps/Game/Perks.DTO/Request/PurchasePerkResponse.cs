using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class PurchasePerkResponse : ModuleResponse
    {
        [JsonProperty]
        public bool Success { get; set; }

        [JsonProperty]
        public string UnlockedPerkId { get; set; } = string.Empty;

        [JsonProperty]
        public long NextCost { get; set; }

        [JsonProperty]
        public long Cost { get; set; }

        [JsonProperty]
        public long NewGoldBalance { get; set; }
    }
}
