using GameModuleDTO.Modules.Currency;
using Newtonsoft.Json;

namespace GameModuleDTO.ModuleRequests
{
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
