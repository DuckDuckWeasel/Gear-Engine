using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Roguelike
{
    public sealed class RoguelikeConfig
    {
        [JsonProperty("gearPool")]
        public List<string> GearPool { get; set; } = new List<string>();

        [JsonProperty("optionsPerRoll")]
        public int OptionsPerRoll { get; set; } = 3;
    }
}
