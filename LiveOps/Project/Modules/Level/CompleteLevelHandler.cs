using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Gold;
using GameModuleDTO.Modules.Level;
using Microsoft.Extensions.Logging;

namespace GameModule.Modules.Level
{
    /// <summary>
    /// GameApi handler for <see cref="CompleteLevelRequest"/>.
    /// </summary>
    public sealed class CompleteLevelHandler : IGameApiHandler<CompleteLevelRequest, CompleteLevelResponse>
    {
        private readonly ILogger<CompleteLevelHandler> _logger;

        public CompleteLevelHandler(ILogger<CompleteLevelHandler> logger)
        {
            _logger = logger;
        }

        public async Task<CompleteLevelResponse> HandleAsync(GameApiSession session, CompleteLevelRequest request)
        {
            LevelConfig config = await session.RemoteConfig.Get(session.Context, new LevelConfig()).ConfigureAwait(false);
            LevelPersistence persistence = await session.Player.GetOrSet(session.Context, new LevelPersistence()).ConfigureAwait(false);

            IReadOnlyList<int> levels = config.Levels;
            int index = IndexOfLevelId(levels, request.LevelId);
            if (index < 0)
            {
                _logger.LogWarning("[CompleteLevelHandler] Attempted to complete level {AttemptedLevel} but it is not in the valid levels list", request.LevelId);
                return SnapshotResponse(false, persistence, config, null);
            }

            HashSet<int> completed = new HashSet<int>(persistence.CompletedLevelIds);
            if (completed.Contains(request.LevelId))
            {
                _logger.LogWarning("[CompleteLevelHandler] Level {LevelId} is already completed", request.LevelId);
                return SnapshotResponse(false, persistence, config, null);
            }

            if (index > 0)
            {
                int previousId = levels[index - 1];
                if (!completed.Contains(previousId))
                {
                    _logger.LogWarning("[CompleteLevelHandler] Previous level {PreviousId} is not completed for {AttemptedLevel}", previousId, request.LevelId);
                    return SnapshotResponse(false, persistence, config, null);
                }
            }

            persistence.AddCompletedLevel(request.LevelId);

            int reward = config.RewardPerLevel;
            if (reward > 0)
            {
                await session.InvokeAsync<AddGoldRequest, GoldChangedResponse>(new AddGoldRequest(reward)).ConfigureAwait(false);
            }

            _logger.LogInformation("[CompleteLevelHandler] Level {LevelId} completed successfully for player {PlayerId}", request.LevelId, session.Context.PlayerId);

            return SnapshotResponse(true, persistence, config, request.LevelId);
        }

        private static CompleteLevelResponse SnapshotResponse(bool succeeded, LevelPersistence persistence, LevelConfig config, int? completedLevelId)
        {
            LevelGameData data = new LevelGameData(persistence, config);
            return new CompleteLevelResponse(succeeded, data, completedLevelId);
        }

        private static int IndexOfLevelId(IReadOnlyList<int> levels, int levelId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] == levelId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
