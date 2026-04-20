using GameModuleDTO.GameApi;
using GameModuleDTO.Modules.Currency;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
    [UsesGameApi]
    public sealed class SpendCurrencyRequest : ModuleRequest<SpendCurrencyResponse>
    {
        public SpendCurrencyRequest()
        {
        }

        public SpendCurrencyRequest(string currencyId, long amount)
        {
            CurrencyId = currencyId;
            Amount = amount;
        }

        [JsonProperty]
        public string CurrencyId { get; set; } = string.Empty;

        [JsonProperty]
        public long Amount { get; set; }
    }
}
