using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Roguelike
{
    [LiveOpsKey(nameof(RoguelikeConfig))]
    public sealed class RoguelikeConfig
    {
        [JsonProperty("gearPool")]
        public List<string> GearPool { get; set; } = new List<string>();

        [JsonProperty("optionsPerRoll")]
        public int OptionsPerRoll { get; set; } = 3;
    }
}
