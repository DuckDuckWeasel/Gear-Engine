using GameModuleDTO.ModuleRequests;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Currency
{
    public sealed class CurrencyChangedResponse : ModuleResponse
    {
        public CurrencyChangedResponse()
        {
        }

        public CurrencyChangedResponse(string currencyId, long newAmount, long diff)
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
