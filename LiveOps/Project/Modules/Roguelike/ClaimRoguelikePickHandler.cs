using System.Collections.Generic;
using System.Threading.Tasks;
using GameModule.GameApi;
using GameModule.ModuleFetchData;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Roguelike;

namespace GameModule.Modules.Roguelike
{
    public sealed class ClaimRoguelikePickHandler : IGameApiHandler<ClaimRoguelikePickRequest, ClaimRoguelikePickResponse>
    {
        public async Task<ClaimRoguelikePickResponse> HandleAsync(GameApiSession session, ClaimRoguelikePickRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PickedGearId))
            {
                return new ClaimRoguelikePickResponse { Success = false };
            }

            RoguelikePersistence persistence = await session.Player.Get(session.Context, RoguelikeModule.PersistenceKey, new RoguelikePersistence()).ConfigureAwait(false);
            if (!persistence.CurrentRollIds.Contains(request.PickedGearId))
            {
                return new ClaimRoguelikePickResponse { Success = false };
            }

            persistence.CurrentRollIds = new List<string>();
            return new ClaimRoguelikePickResponse { Success = true };
        }
    }
}
