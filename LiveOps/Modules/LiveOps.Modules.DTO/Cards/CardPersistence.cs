using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Cards
{
    [LiveOpsKey(nameof(CardPersistence))]
    public sealed class CardPersistence
    {
        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();
    }
}
