using GameModuleDTO.GameApi;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class PurchaseCardRequest : ModuleRequest<PurchaseCardResponse>
    {
    }
}
