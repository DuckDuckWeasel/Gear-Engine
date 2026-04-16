using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    /// <summary>
    /// One card slot in player persistence (backend shape; Unity mirrors for local play).
    /// </summary>
    public sealed class CardSlotEntry
    {
        [JsonProperty("slotIndex")]
        public int SlotIndex { get; set; }

        [JsonProperty("state")]
        public CardSlotStateDto State { get; set; }

        /// <summary>Assigned card catalog id when <see cref="State"/> is <see cref="CardSlotStateDto.Collected"/>.</summary>
        [JsonProperty("cardId")]
        public string CardId { get; set; }
    }
}
