using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Perks
{
    [LiveOpsKey(nameof(PerkPersistence))]
    public sealed class PerkPersistence
    {
        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();
    }
}
