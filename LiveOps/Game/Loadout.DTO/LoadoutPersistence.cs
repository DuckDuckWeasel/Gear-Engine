using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Loadout
{
    [LiveOpsKey(nameof(LoadoutPersistence))]
    public sealed class LoadoutPersistence
    {
        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();
    }
}
