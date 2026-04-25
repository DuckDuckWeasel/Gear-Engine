using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Inventory
{
    [LiveOpsKey(nameof(InventoryPersistence))]
    public sealed class InventoryPersistence
    {
        [JsonProperty("gears")]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();

        // One-shot guard: set to true after InventoryModule seeds ordered startingGearIds (index 0 = motor).
        // so a player who removes a starter gear does not have it reinserted on the next session.
        [JsonProperty("startingGearsSeeded")]
        public bool StartingGearsSeeded { get; set; }
    }
}
