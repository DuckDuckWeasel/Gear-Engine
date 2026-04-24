using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Loadout
{
    public sealed class LoadoutPersistence
    {
        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();
    }
}
