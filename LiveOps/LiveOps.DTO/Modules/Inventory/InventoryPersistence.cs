using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class InventoryPersistence
    {
        [JsonProperty("gearIds")]
        public List<string> GearIds { get; set; } = new List<string>();
    }
}
