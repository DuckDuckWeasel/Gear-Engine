using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Level;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Level
{
    /// <summary>
    /// Cloud Code level module: persistence + remote config merged into <see cref="LevelGameData"/>.
    /// </summary>
    public class LevelService : GameModule<LevelGameData>
    {
        private readonly ILogger<LevelService> _logger;

        public LevelService(ILogger<LevelService> logger)
        {
            _logger = logger;
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData Player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            _logger.LogTrace("[LevelService] Initialize");
            LevelPersistence persistence = await Player.GetOrSet(context, new LevelPersistence());
            LevelConfig config = await remoteConfig.Get(context, new LevelConfig());
            return new LevelGameData(persistence, config);
        }
    }
}
