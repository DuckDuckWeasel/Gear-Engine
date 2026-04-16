using System.Threading;
using System.Threading.Tasks;

namespace GearEngine.Race.Rewards
{
    public sealed class StubRaceRewardLiveOpsClient : IRaceRewardLiveOpsClient
    {
        private int goldBalance;

        public int GoldBalance => goldBalance;

        public Task<RaceRewardGrantResult> GrantRaceRewardAsync(RaceRewardGrantRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.GoldAmount < 0)
            {
                return Task.FromResult(new RaceRewardGrantResult(false, goldBalance, "Invalid gold amount."));
            }

            goldBalance += request.GoldAmount;
            return Task.FromResult(new RaceRewardGrantResult(true, goldBalance, "Stub grant applied."));
        }
    }
}
