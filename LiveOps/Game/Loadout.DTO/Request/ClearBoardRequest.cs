using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(ClearBoardRequest))]
    public sealed class ClearBoardRequest : ModuleRequest<ClearBoardResponse>
    {
    }
}
