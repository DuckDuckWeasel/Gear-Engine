using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    public sealed class CardGameData : IGameModuleData
    {
        public string Key => nameof(CardGameData);

        [JsonProperty("unlocked")]
        public List<string> Unlocked { get; set; } = new List<string>();

        [JsonProperty("nextCost")]
        public long NextCost { get; set; }

        [JsonProperty("currencyId")]
        public string CurrencyId { get; set; } = string.Empty;

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
            CurrencyId = config.CurrencyId;
        }
    }
}
