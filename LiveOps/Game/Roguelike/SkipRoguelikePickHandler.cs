using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Roguelike;

namespace LiveOps.Modules.Roguelike
{
    public sealed class SkipRoguelikePickHandler : IGameApiHandler<SkipRoguelikePickRequest, SkipRoguelikePickResponse>
    {
        public async Task<SkipRoguelikePickResponse> HandleAsync(GameApiSession session, SkipRoguelikePickRequest request)
        {
            RoguelikePersistence persistence = await session.Player.Get(session.Context, RoguelikeModule.PersistenceKey, new RoguelikePersistence());
            
            // Clear current roll to allow drawing again next time
            persistence.CurrentRollIds = new List<string>();
            
            return new SkipRoguelikePickResponse { Success = true };
        }
    }
}
