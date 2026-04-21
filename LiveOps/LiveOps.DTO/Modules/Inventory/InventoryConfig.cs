using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
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
    }
}
