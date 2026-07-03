using System;
using System.Collections.Generic;

namespace GearEngine.Campaign.Presentation
{
    [Serializable]
    public class TierSlotPrefabConfig
    {
        public List<int> Tiers;
        public TrackTierSlotView Prefab;

        public bool Contains(int tier) => Tiers != null && Tiers.Contains(tier);
    }
}
