using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Loadout;

namespace GameModule.Modules.Loadout
{
    public sealed class SaveBoardLayoutHandler : IGameApiHandler<SaveBoardLayoutRequest, SaveBoardLayoutResponse>
    {
        public async Task<SaveBoardLayoutResponse> HandleAsync(GameApiSession session, SaveBoardLayoutRequest request)
        {
            LoadoutPersistence persistence = await session.Player.Get(session.Context, LoadoutModule.PersistenceKey, new LoadoutPersistence()).ConfigureAwait(false);
            persistence.Board = (request?.Placements ?? new List<LoadoutPlacement>())
                .Where(x => x != null && !string.IsNullOrEmpty(x.GearId))
                .ToList();
            return new SaveBoardLayoutResponse { SavedAtUtcTicks = DateTime.UtcNow.Ticks };
        }
    }
}
