using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.ModuleFetchData;
using LiveOps.Modules.DTO.Loadout;
using LiveOps.Modules.DTO.ModuleRequests;

namespace LiveOps.Modules.Loadout
{
    public sealed class ClearBoardHandler : IGameApiHandler<ClearBoardRequest, ClearBoardResponse>
    {
        public async Task<ClearBoardResponse> HandleAsync(GameApiSession session, ClearBoardRequest request)
        {
            LoadoutPersistence persistence = await session.Player.Get(session.Context, LoadoutModule.PersistenceKey, new LoadoutPersistence());
            persistence.Board.Clear();

            await session.Player.Set(session.Context, LoadoutModule.PersistenceKey, persistence);
            return new ClearBoardResponse();
        }
    }
}
