using GameModuleDTO.ModuleRequests;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards.Request
{
    /// <summary>
    /// Stub request: server will validate gold, cost curve, and assign a random card id.
    /// </summary>
    public sealed class PurchaseCardSlotRequest : ModuleRequest<PurchaseCardSlotResponse>
    {
        [JsonProperty("slotIndex")]
        public int SlotIndex { get; set; }
    }
}
