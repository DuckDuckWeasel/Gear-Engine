using System.Threading.Tasks;
using GameModule.GameModule;
using GameModule.ModuleFetchData;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Inventory;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Inventory
{
    public class InventoryModule : GameModule<InventoryGameData>
    {
        public const string PersistenceKey = nameof(InventoryPersistence);
        public const string ConfigKey = nameof(InventoryConfig);

        public InventoryModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            InventoryConfig config = await remoteConfig.Get(context, ConfigKey, new InventoryConfig()).ConfigureAwait(false);
            InventoryPersistence persistence = await player.Get(context, PersistenceKey, new InventoryPersistence()).ConfigureAwait(false);
            return new InventoryGameData(persistence, config);
        }
    }
}
