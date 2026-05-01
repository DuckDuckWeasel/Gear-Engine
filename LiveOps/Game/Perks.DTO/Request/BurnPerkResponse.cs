using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    public sealed class BurnPerkResponse : ModuleResponse
    {
        [JsonProperty]
        public bool Success { get; set; }

        /// <summary>Gold earned from burning the card.</summary>
        [JsonProperty]
        public long GoldEarned { get; set; }

        /// <summary>Player's new gold balance after the burn reward.</summary>
        [JsonProperty]
        public long NewGoldBalance { get; set; }
    }
}
