using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [LiveOpsKey(nameof(AddCurrencyRequest))]
    public sealed class AddCurrencyRequest : ModuleRequest<AddCurrencyResponse>
    {
        public AddCurrencyRequest()
        {
        }

        public AddCurrencyRequest(string currencyId, long amount)
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
