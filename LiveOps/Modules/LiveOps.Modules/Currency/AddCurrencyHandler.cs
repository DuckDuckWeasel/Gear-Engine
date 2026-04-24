using System.Threading.Tasks;
using LiveOps.GameApi;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;

namespace LiveOps.Modules.Currency
{
    public sealed class AddCurrencyHandler : IGameApiHandler<AddCurrencyRequest, AddCurrencyResponse>
    {
        private readonly CurrencyModule _currencyModule;

        public AddCurrencyHandler(CurrencyModule currencyModule)
        {
            _currencyModule = currencyModule;
        }

        public async Task<AddCurrencyResponse> HandleAsync(GameApiSession session, AddCurrencyRequest request)
        {
            string id = request?.CurrencyId ?? string.Empty;
            long amount = request?.Amount ?? 0;
            CurrencyChangedResponse change = await _currencyModule.AddToPlayer(session.Context, session.Player, session.RemoteConfig, id, amount);
            return new AddCurrencyResponse(change.CurrencyId, change.NewAmount, change.Diff);
        }
    }
}
