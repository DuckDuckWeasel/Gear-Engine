using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Cards
{
    public sealed class CardConfig
    {
        [JsonProperty("catalog")]
        public List<string> Catalog { get; set; } = new List<string>();

        [JsonProperty("currencyId")]
        public string CurrencyId { get; set; } = "gold";

        [JsonProperty("baseCost")]
        public long BaseCost { get; set; } = 100;

        [JsonProperty("costPerPurchaseGrowth")]
        public long CostPerPurchaseGrowth { get; set; } = 50;

        public long CostFor(int alreadyUnlockedCount)
        {
            return BaseCost + CostPerPurchaseGrowth * alreadyUnlockedCount;
        }
    }
}
