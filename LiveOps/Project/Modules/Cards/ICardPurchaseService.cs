using System.Threading.Tasks;
using GameModuleDTO.Modules.Cards.Request;
using Unity.Services.CloudCode.Core;

namespace GameModule.Modules.Cards
{
    /// <summary>
    /// Backend hook for gold spend + random roll + persistence (implement in Cloud Code module later).
    /// </summary>
    public interface ICardPurchaseService
    {
        Task<PurchaseCardSlotResponse> PurchaseSlotAsync(IExecutionContext context, PurchaseCardSlotRequest request);
    }
}
