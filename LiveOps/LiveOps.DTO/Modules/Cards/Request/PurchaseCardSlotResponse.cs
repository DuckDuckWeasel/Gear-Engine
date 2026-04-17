using GameModuleDTO.ModuleRequests;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards.Request
{
    public sealed class PurchaseCardSlotResponse : ModuleResponse
    {
        [JsonProperty("succeeded")]
        public bool Succeeded { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("slot")]
        public CardSlotEntry Slot { get; set; }

        [JsonProperty("goldRemaining")]
        public long? GoldRemaining { get; set; }
    }
}
