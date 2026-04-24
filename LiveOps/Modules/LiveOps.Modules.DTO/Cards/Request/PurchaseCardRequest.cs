using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("PurchaseCardRequest")]
    public sealed class PurchaseCardRequest : ModuleRequest<PurchaseCardResponse>
    {
    }
}
