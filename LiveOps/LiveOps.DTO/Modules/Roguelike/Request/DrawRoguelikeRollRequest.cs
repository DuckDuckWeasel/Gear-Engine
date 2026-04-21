using GameModuleDTO.GameApi;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class DrawRoguelikeRollRequest : ModuleRequest<DrawRoguelikeRollResponse>
    {
    }
}
