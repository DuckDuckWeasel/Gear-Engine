using GameModuleDTO.GameApi;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class ClearBoardRequest : ModuleRequest<ClearBoardResponse>
    {
    }
}
