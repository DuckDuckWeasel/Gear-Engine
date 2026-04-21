using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModule.Modules.Inventory;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Inventory;
using GameModuleDTO.Modules.Loadout;

namespace GameModule.Modules.Loadout
{
    public sealed class SaveBoardLayoutHandler : IGameApiHandler<SaveBoardLayoutRequest, SaveBoardLayoutResponse>
    {
        public async Task<SaveBoardLayoutResponse> HandleAsync(GameApiSession session, SaveBoardLayoutRequest request)
        {
            InventoryPersistence inventoryPersistence = await session.Player.Get(session.Context, InventoryModule.PersistenceKey, new InventoryPersistence()).ConfigureAwait(false);
            HashSet<string> owned = new HashSet<string>(
                (inventoryPersistence.Gears ?? new List<OwnedGearEntry>())
                    .Where(g => g != null && !string.IsNullOrEmpty(g.InstanceId))
                    .Select(g => g.InstanceId));

            LoadoutPersistence persistence = await session.Player.Get(session.Context, LoadoutModule.PersistenceKey, new LoadoutPersistence()).ConfigureAwait(false);
            persistence.Board = (request?.Placements ?? new List<LoadoutPlacement>())
                .Where(x => x != null && !string.IsNullOrEmpty(x.InstanceId) && owned.Contains(x.InstanceId))
                .ToList();

            // Player.Get returns a default instance that is not tracked in the cache when the key is missing;
            // FlushDirtyAsync only persists entries in _objectCache, so we must Set explicitly (see GameApiDispatcher).
            await session.Player.Set(session.Context, LoadoutModule.PersistenceKey, persistence).ConfigureAwait(false);
            return new SaveBoardLayoutResponse { SavedAtUtcTicks = DateTime.UtcNow.Ticks };
        }
    }
}
