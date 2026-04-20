using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Currency
{
    public sealed class CurrencyConfigEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("initial")]
        public long Initial { get; set; }

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public long? Min { get; set; }

        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public long? Max { get; set; }
    }
}
