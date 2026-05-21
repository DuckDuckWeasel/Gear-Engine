using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(SkipRoguelikePickRequest))]
    public sealed class SkipRoguelikePickRequest : ModuleRequest<SkipRoguelikePickResponse>
    {
    }
}
