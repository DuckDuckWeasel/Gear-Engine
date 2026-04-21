using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class InventoryPersistence
    {
        [JsonProperty("gears")]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();
    }
}
