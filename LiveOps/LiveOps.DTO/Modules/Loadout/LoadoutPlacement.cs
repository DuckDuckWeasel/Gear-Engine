using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Loadout
{
    public sealed class LoadoutPlacement
    {
        [JsonProperty("gearId")]
        public string GearId { get; set; } = string.Empty;

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
