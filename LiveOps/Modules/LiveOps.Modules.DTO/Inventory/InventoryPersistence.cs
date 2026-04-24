using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Inventory
{
    public sealed class InventoryPersistence
    {
        [JsonProperty("gears")]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();

        // One-shot guard: set to true after InventoryModule seeds InventoryConfig.StartingGearIds
        // so a player who removes a starter gear does not have it reinserted on the next session.
        [JsonProperty("startingGearsSeeded")]
        public bool StartingGearsSeeded { get; set; }
    }
}
