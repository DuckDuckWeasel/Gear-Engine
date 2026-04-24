using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Roguelike
{
    public sealed class RoguelikePersistence
    {
        [JsonProperty("currentRollIds")]
        public List<string> CurrentRollIds { get; set; } = new List<string>();
    }
}
