using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Roguelike
{
    [LiveOpsKey(nameof(RoguelikePersistence))]
    public sealed class RoguelikePersistence
    {
        [JsonProperty("currentRollIds")]
        public List<string> CurrentRollIds { get; set; } = new List<string>();
    }
}
