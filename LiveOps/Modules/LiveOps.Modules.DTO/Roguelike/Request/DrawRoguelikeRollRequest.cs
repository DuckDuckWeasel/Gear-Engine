using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(DrawRoguelikeRollRequest))]
    public sealed class DrawRoguelikeRollRequest : ModuleRequest<DrawRoguelikeRollResponse>
    {
    }
}
