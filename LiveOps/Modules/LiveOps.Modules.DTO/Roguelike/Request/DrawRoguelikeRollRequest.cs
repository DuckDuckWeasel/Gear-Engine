using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("DrawRoguelikeRollRequest")]
    public sealed class DrawRoguelikeRollRequest : ModuleRequest<DrawRoguelikeRollResponse>
    {
    }
}
