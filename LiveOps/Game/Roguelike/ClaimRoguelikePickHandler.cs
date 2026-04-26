using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.ModuleFetchData;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Roguelike;

namespace LiveOps.Modules.Roguelike
{
    public sealed class ClaimRoguelikePickHandler : IGameApiHandler<ClaimRoguelikePickRequest, ClaimRoguelikePickResponse>
    {
        public async Task<ClaimRoguelikePickResponse> HandleAsync(GameApiSession session, ClaimRoguelikePickRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PickedGearId))
            {
                return new ClaimRoguelikePickResponse { Success = false };
            }

            RoguelikePersistence persistence = await session.Player.Get(session.Context, RoguelikeModule.PersistenceKey, new RoguelikePersistence());
            if (!persistence.CurrentRollIds.Contains(request.PickedGearId))
            {
                return new ClaimRoguelikePickResponse { Success = false };
            }

            persistence.CurrentRollIds = new List<string>();
            return new ClaimRoguelikePickResponse { Success = true };
        }
    }
}
