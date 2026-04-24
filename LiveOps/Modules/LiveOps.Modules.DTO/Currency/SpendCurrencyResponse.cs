using LiveOps.DTO.ModuleRequest;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Currency
{
    public sealed class SpendCurrencyResponse : ModuleResponse
    {
        public SpendCurrencyResponse()
        {
        }

        public SpendCurrencyResponse(string currencyId, long newAmount, long spent, bool succeeded)
        {
            CurrencyId = currencyId;
            NewAmount = newAmount;
            Spent = spent;
            Succeeded = succeeded;
        }

        [JsonProperty]
        public string CurrencyId { get; set; } = string.Empty;

        [JsonProperty]
        public long NewAmount { get; set; }

        [JsonProperty]
        public long Spent { get; set; }

        [JsonProperty]
        public bool Succeeded { get; set; }
    }
}
