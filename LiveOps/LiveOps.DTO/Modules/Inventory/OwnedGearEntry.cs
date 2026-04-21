using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class OwnedGearEntry
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; } = string.Empty;

        [JsonProperty("gearId")]
        public string GearId { get; set; } = string.Empty;
    }
}
