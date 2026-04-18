namespace GearEngine.Campaign
{
    public sealed class GoldReward
    {
        private const int goldPerScorePoint = 5;

        public GoldReward(int score)
        {
            Amount = score * goldPerScorePoint;
        }

        public int Amount { get; }
    }
}
