using System.Threading.Tasks;
using GameModule.GameApi;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Currency;

namespace GameModule.Modules.Currency
{
    public sealed class SpendCurrencyHandler : IGameApiHandler<SpendCurrencyRequest, SpendCurrencyResponse>
    {
        private readonly CurrencyModule _currencyModule;

        public SpendCurrencyHandler(CurrencyModule currencyModule)
        {
            _currencyModule = currencyModule;
        }

        public async Task<SpendCurrencyResponse> HandleAsync(GameApiSession session, SpendCurrencyRequest request)
        {
            string id = request?.CurrencyId ?? string.Empty;
            long amount = request?.Amount ?? 0;
            (bool ok, CurrencyChangedResponse change) = await _currencyModule.TrySpendFromPlayer(session.Context, session.Player, session.RemoteConfig, id, amount).ConfigureAwait(false);
            return new SpendCurrencyResponse(id, change.NewAmount, ok ? amount : 0, ok);
        }
    }
}
