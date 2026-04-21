using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Loadout
{
    public sealed class LoadoutConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 6;
    }
}
