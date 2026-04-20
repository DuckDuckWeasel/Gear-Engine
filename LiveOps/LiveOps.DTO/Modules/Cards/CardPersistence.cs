using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    public sealed class CardPersistence
    {
        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();
    }
}
