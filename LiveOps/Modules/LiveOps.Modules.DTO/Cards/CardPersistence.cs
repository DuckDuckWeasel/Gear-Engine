using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Cards
{
    public sealed class CardPersistence
    {
        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();
    }
}
