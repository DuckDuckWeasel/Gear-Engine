using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(RerollRoguelikeRollRequest))]
    public sealed class RerollRoguelikeRollRequest : ModuleRequest<RerollRoguelikeRollResponse>
    {
    }
}
