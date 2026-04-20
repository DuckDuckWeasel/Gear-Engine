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

        public InventoryModule()
        {
        }

        public override async Task<IGameModuleData> Initialize(IExecutionContext context, IPlayerData player, IGameState gameState, IRemoteConfig remoteConfig)
        {
            InventoryPersistence persistence = await player.Get(context, PersistenceKey, new InventoryPersistence()).ConfigureAwait(false);
            return new InventoryGameData(persistence);
        }
    }
}
