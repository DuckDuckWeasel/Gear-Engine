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

            var seen = new HashSet<string>();
            persistence.Gears = (request?.Gears ?? new List<OwnedGearEntry>())
                .Where(g => g != null && !string.IsNullOrEmpty(g.GearId) && !string.IsNullOrEmpty(g.InstanceId) && seen.Add(g.InstanceId))
                .Select(g => new OwnedGearEntry { InstanceId = g.InstanceId, GearId = g.GearId })
                .ToList();

            // Player.Get returns a default instance that is not tracked in the cache when the key is missing;
            // FlushDirtyAsync only persists entries in _objectCache, so we must Set explicitly (see GameApiDispatcher).
            await session.Player.Set(session.Context, InventoryModule.PersistenceKey, persistence).ConfigureAwait(false);
            return new SetInventoryResponse { Gears = new List<OwnedGearEntry>(persistence.Gears) };
        }
    }
}
