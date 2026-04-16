using System.Threading;
using System.Threading.Tasks;

namespace GearEngine.Race.Rewards
{
    public interface IRaceRewardLiveOpsClient
    {
        Task<RaceRewardGrantResult> GrantRaceRewardAsync(RaceRewardGrantRequest request, CancellationToken cancellationToken = default);
    }

    public readonly struct RaceRewardGrantRequest
    {
        public RaceRewardGrantRequest(string trackId, string rankId, int goldAmount, float clientFinishTimeSeconds)
        {
            TrackId = trackId ?? string.Empty;
            RankId = rankId ?? string.Empty;
            GoldAmount = goldAmount;
            ClientFinishTimeSeconds = clientFinishTimeSeconds;
        }

        public string TrackId { get; }

        public string RankId { get; }

        public int GoldAmount { get; }

        public float ClientFinishTimeSeconds { get; }
    }

    public readonly struct RaceRewardGrantResult
    {
        public RaceRewardGrantResult(bool success, int newGoldBalance, string message)
        {
            Success = success;
            NewGoldBalance = newGoldBalance;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }

        public int NewGoldBalance { get; }

        public string Message { get; }
    }
}
