using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Inventory;

namespace GameModule.Modules.Inventory
{
    public sealed class SetInventoryHandler : IGameApiHandler<SetInventoryRequest, SetInventoryResponse>
    {
        public async Task<SetInventoryResponse> HandleAsync(GameApiSession session, SetInventoryRequest request)
        {
            InventoryPersistence persistence = await session.Player.Get(session.Context, InventoryModule.PersistenceKey, new InventoryPersistence()).ConfigureAwait(false);
            persistence.GearIds = (request?.GearIds ?? new List<string>()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            // Player.Get returns a default instance that is not tracked in the cache when the key is missing;
            // FlushDirtyAsync only persists entries in _objectCache, so we must Set explicitly (see GameApiDispatcher).
            await session.Player.Set(session.Context, InventoryModule.PersistenceKey, persistence).ConfigureAwait(false);
            return new SetInventoryResponse { GearIds = new List<string>(persistence.GearIds) };
        }
    }
}
