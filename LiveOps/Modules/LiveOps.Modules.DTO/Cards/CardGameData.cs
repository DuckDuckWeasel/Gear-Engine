using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Cards
{
    [LiveOpsKey(nameof(CardGameData))]
    public sealed class CardGameData : IGameModuleData
    {
        public string Key => nameof(CardGameData);

        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();

        [JsonProperty("nextCost")]
        public long NextCost { get; set; }

        [JsonConstructor]
        private CardGameData()
        {
        }

        public CardGameData(CardPersistence persistence, CardConfig config)
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
        }
    }
}
