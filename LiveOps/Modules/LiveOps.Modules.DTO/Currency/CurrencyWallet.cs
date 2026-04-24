using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Currency
{
    public sealed class CurrencyWallet
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("current")]
        public long Current { get; set; }

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public long? Min { get; set; }

        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public long? Max { get; set; }

        public bool CanSpend(long amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            long floor = Min ?? 0;
            return Current - amount >= floor;
        }
    }
}
