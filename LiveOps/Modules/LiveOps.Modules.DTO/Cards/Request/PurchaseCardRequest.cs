using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(PurchaseCardRequest))]
    public sealed class PurchaseCardRequest : ModuleRequest<PurchaseCardResponse>
    {
    }
}
