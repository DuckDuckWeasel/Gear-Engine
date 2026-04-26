using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Currency
{
    public sealed class AddCurrencyResponse : ModuleResponse
    {
        public AddCurrencyResponse()
        {
        }

        public AddCurrencyResponse(string currencyId, long newAmount, long diff)
        {
            CurrencyId = currencyId;
            NewAmount = newAmount;
            Diff = diff;
        }

        [JsonProperty]
        public string CurrencyId { get; set; } = string.Empty;

        [JsonProperty]
        public long NewAmount { get; set; }

        [JsonProperty]
        public long Diff { get; set; }
    }
}
