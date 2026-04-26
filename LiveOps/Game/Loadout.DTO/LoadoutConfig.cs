using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Loadout
{
    [LiveOpsKey(nameof(LoadoutConfig))]
    public sealed class LoadoutConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 6;

        [JsonProperty("motorCogStartX")]
        public int MotorCogStartX { get; set; } = 2;

        [JsonProperty("motorCogStartY")]
        public int MotorCogStartY { get; set; } = 2;
    }
}
