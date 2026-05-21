using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Perks
{
    [LiveOpsKey(nameof(PerkConfig))]
    public sealed class PerkConfig
    {
        [JsonProperty("catalog")]
        public List<string> Catalog { get; set; } = new List<string>();

        [JsonProperty("baseCost")]
        public long BaseCost { get; set; } = 100;

        [JsonProperty("costPerPurchaseGrowth")]
        public long CostPerPurchaseGrowth { get; set; } = 50;

        /// <summary>Gold refunded when a player burns (destroys) one copy of a card.</summary>
        [JsonProperty("burnReward")]
        public long BurnReward { get; set; } = 50;

        public long CostFor(int alreadyOwnedCount)
        {
            return BaseCost + CostPerPurchaseGrowth * alreadyOwnedCount;
        }
    }
}

