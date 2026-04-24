using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Loadout
{
    public sealed class LoadoutConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 6;
    }
}
