using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("ClearBoardRequest")]
    public sealed class ClearBoardRequest : ModuleRequest<ClearBoardResponse>
    {
    }
}
