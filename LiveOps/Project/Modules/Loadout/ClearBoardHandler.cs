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

            // Same as SaveBoardLayoutHandler: untracked default persistence is not flushed unless Set is called.
            await session.Player.Set(session.Context, LoadoutModule.PersistenceKey, persistence).ConfigureAwait(false);
            return new ClearBoardResponse();
        }
    }
}
