using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(SpendCurrencyRequest))]
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
