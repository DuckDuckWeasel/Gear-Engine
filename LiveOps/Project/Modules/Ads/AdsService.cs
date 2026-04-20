using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Ads;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Ads
{
    /// <summary>
    /// Cloud Code ads module: persistence + remote config merged into <see cref="AdData"/>.
    /// </summary>
    public class AdsService : GameModule<AdData>
    {
        private readonly ILogger<AdsService> _logger;

        public AdsService(ILogger<AdsService> logger)
        {
            _logger = logger;
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData Player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            _logger.LogTrace("[AdsService] Initialize");
            AdsPersistence persistence = await Player.GetOrSet(context, new AdsPersistence());
            AdsConfig config = await remoteConfig.Get(context, new AdsConfig());
            return new AdData(persistence, config);
        }
    }
}
