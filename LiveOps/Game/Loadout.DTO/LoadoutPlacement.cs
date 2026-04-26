using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Loadout
{
    public sealed class LoadoutPlacement
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; } = string.Empty;

        [JsonProperty("gearId")]
        public string GearId { get; set; } = string.Empty;

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
