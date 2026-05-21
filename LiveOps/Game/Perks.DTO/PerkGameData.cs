using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Perks
{
    [LiveOpsKey(nameof(PerkGameData))]
    public sealed class PerkGameData : IGameModuleData
    {
        public string Key => nameof(PerkGameData);

        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();

        [JsonProperty("nextCost")]
        public long NextCost { get; set; }

        /// <summary>Gold returned when burning one copy of any Perk.</summary>
        [JsonProperty("burnReward")]
        public long BurnReward { get; set; }

        [JsonConstructor]
        private PerkGameData()
        {
        }

        public PerkGameData(PerkPersistence persistence, PerkConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Unlocked = new List<string>(persistence.Unlocked);
            NextCost = config.CostFor(Unlocked.Count);
            BurnReward = config.BurnReward;
        }
    }
}

