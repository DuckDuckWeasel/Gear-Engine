using LiveOps.DTO.ModuleRequest;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(PurchasePerkRequest))]
    public sealed class PurchasePerkRequest : ModuleRequest<PurchasePerkResponse>
    {
    }
}
