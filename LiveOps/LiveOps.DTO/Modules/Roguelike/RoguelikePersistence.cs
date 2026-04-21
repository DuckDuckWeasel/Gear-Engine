using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Roguelike
{
    public sealed class RoguelikePersistence
    {
        [JsonProperty("currentRollIds")]
        public List<string> CurrentRollIds { get; set; } = new List<string>();
    }
}
