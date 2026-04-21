using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class InventoryConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 8;
    }
}
