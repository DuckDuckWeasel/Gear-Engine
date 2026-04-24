using LiveOps.DTO.GameApi;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.ModuleRequests
{
    [UsesGameApi]
    [GameApiKey("AddCurrencyRequest")]
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
