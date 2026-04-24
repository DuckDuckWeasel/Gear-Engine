using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Inventory
{
    public sealed class InventoryConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 8;

        [JsonProperty("motorCogGearId")]
        public string MotorCogGearId { get; set; } = "gear_core";

        [JsonProperty("motorCogStartX")]
        public int MotorCogStartX { get; set; } = 2;

        [JsonProperty("motorCogStartY")]
        public int MotorCogStartY { get; set; } = 2;

        // Catalog ids handed to brand-new players in addition to the motor cog.
        // Seeded once on first Initialize (gated by InventoryPersistence.StartingGearsSeeded)
        // so removed gears do not respawn on subsequent logins.
        [JsonProperty("startingGearIds")]
        public List<string> StartingGearIds { get; set; } = new List<string>();
    }
}
