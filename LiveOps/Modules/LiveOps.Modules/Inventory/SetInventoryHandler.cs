using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.ModuleFetchData;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.ModuleRequests;

namespace LiveOps.Modules.Inventory
{
    public sealed class SetInventoryHandler : IGameApiHandler<SetInventoryRequest, SetInventoryResponse>
    {
        public async Task<SetInventoryResponse> HandleAsync(GameApiSession session, SetInventoryRequest request)
        {
            InventoryPersistence persistence = await session.Player.Get(session.Context, InventoryModule.PersistenceKey, new InventoryPersistence());

            var seen = new HashSet<string>();
            persistence.Gears = (request?.Gears ?? new List<OwnedGearEntry>())
                .Where(g => g != null && !string.IsNullOrEmpty(g.GearId) && !string.IsNullOrEmpty(g.InstanceId) && seen.Add(g.InstanceId))
                .Select(g => new OwnedGearEntry { InstanceId = g.InstanceId, GearId = g.GearId })
                .ToList();

            await session.Player.Set(session.Context, InventoryModule.PersistenceKey, persistence);
            return new SetInventoryResponse { Gears = new List<OwnedGearEntry>(persistence.Gears) };
        }
    }
}
