using System;
using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Gold;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Gold
{
    /// <summary>
    /// Cloud Code gold module: persistence + remote config merged into <see cref="GoldGameData"/>.
    /// </summary>
    public class GoldModule : GameModule<GoldGameData>
    {
        private readonly ILogger<GoldModule> _logger;

        public GoldModule(ILogger<GoldModule> logger)
        {
            _logger = logger;
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData Player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            _logger.LogInformation("Initializing GoldModule");

            GoldConfig config = await remoteConfig.Get(context, new GoldConfig());
            GoldPersistence persistence = await Player.GetOrSet(context, new GoldPersistence());

            long clamped = Math.Clamp(persistence.Current, config.Min, config.Max);
            if (clamped != persistence.Current)
            {
                persistence.SetCurrent(clamped);
                await Player.Set(context, persistence);
            }

            return new GoldGameData(persistence, config);
        }
    }
}
