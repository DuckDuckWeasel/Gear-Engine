using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Roguelike
{
    [LiveOpsKey(nameof(RoguelikeGameData))]
    public sealed class RoguelikeGameData : IGameModuleData
    {
        public string Key => nameof(RoguelikeGameData);

        [JsonProperty("currentRollIds")]
        public List<string> CurrentRollIds { get; set; } = new List<string>();

        [JsonProperty("optionsPerRoll")]
        public int OptionsPerRoll { get; set; }

        [JsonConstructor]
        private RoguelikeGameData()
        {
        }

        public RoguelikeGameData(RoguelikePersistence persistence, RoguelikeConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            CurrentRollIds = new List<string>(persistence.CurrentRollIds);
            OptionsPerRoll = config.OptionsPerRoll;
        }
    }
}
