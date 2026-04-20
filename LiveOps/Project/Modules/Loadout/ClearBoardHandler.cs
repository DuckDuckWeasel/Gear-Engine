using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Loadout;

namespace GameModule.Modules.Loadout
{
    public sealed class ClearBoardHandler : IGameApiHandler<ClearBoardRequest, ClearBoardResponse>
    {
        public async Task<ClearBoardResponse> HandleAsync(GameApiSession session, ClearBoardRequest request)
        {
            LoadoutPersistence persistence = await session.Player.Get(session.Context, LoadoutModule.PersistenceKey, new LoadoutPersistence()).ConfigureAwait(false);
            persistence.Board.Clear();
            return new ClearBoardResponse();
        }
    }
}
