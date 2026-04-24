using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.ModuleFetchData;
using LiveOps.Modules.DTO.Inventory;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.Inventory;

namespace LiveOps.Modules.Loadout
{
    public sealed class SaveBoardLayoutHandler : IGameApiHandler<SaveBoardLayoutRequest, SaveBoardLayoutResponse>
    {
        public async Task<SaveBoardLayoutResponse> HandleAsync(GameApiSession session, SaveBoardLayoutRequest request)
        {
            InventoryPersistence inventoryPersistence = await session.Player.Get(session.Context, InventoryModule.PersistenceKey, new InventoryPersistence());
            InventoryConfig invConfig = await session.RemoteConfig.Get(session.Context, InventoryModule.ConfigKey, new InventoryConfig());

            HashSet<string> owned = new HashSet<string>(
                (inventoryPersistence.Gears ?? new List<OwnedGearEntry>())
                    .Where(g => g != null && !string.IsNullOrEmpty(g.InstanceId))
                    .Select(g => g.InstanceId));

            List<LoadoutPlacement> placements = (request?.Placements ?? new List<LoadoutPlacement>())
                .Where(x => x != null && !string.IsNullOrEmpty(x.InstanceId) && owned.Contains(x.InstanceId))
                .ToList();

            string coreGearId = invConfig.GetCoreGearCatalogId();
            if (!string.IsNullOrEmpty(coreGearId) &&
                !placements.Any(p => p.GearId == coreGearId))
            {
                return new SaveBoardLayoutResponse
                {
                    Rejected = true,
                    Reason = "missing_motor_cog",
                };
            }

            LoadoutPersistence persistence = await session.Player.Get(session.Context, LoadoutModule.PersistenceKey, new LoadoutPersistence());
            persistence.Board = placements;

            await session.Player.Set(session.Context, LoadoutModule.PersistenceKey, persistence);
            return new SaveBoardLayoutResponse { SavedAtUtcTicks = DateTime.UtcNow.Ticks };
        }
    }
}
