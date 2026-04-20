using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Loadout
{
    public sealed class LoadoutPersistence
    {
        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();
    }
}
