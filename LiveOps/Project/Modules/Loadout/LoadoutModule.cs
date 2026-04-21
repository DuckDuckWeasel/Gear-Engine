using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Loadout;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Loadout
{
    public class LoadoutModule : GameModule<LoadoutGameData>
    {
        public const string PersistenceKey = nameof(LoadoutPersistence);
        public const string ConfigKey = nameof(LoadoutConfig);

        public LoadoutModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            LoadoutConfig config = await remoteConfig.Get(context, ConfigKey, new LoadoutConfig()).ConfigureAwait(false);
            LoadoutPersistence persistence = await player.Get(context, PersistenceKey, new LoadoutPersistence()).ConfigureAwait(false);
            return new LoadoutGameData(persistence, config);
        }
    }
}
