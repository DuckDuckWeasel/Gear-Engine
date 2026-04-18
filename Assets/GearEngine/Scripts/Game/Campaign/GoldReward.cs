using UnityEngine;

namespace GearEngine.Campaign
{
    public sealed class GoldReward
    {
        public GoldReward(int goldAmount)
        {
            Amount = Mathf.Max(0, goldAmount);
        }

        public int Amount { get; }
    }
}
