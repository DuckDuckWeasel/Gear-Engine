using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Currency
{
    [LiveOpsKey(nameof(CurrencyPersistence))]
    public sealed class CurrencyPersistence
    {
        [JsonProperty("balances")]
        private Dictionary<string, long> _balances = new Dictionary<string, long>();

        public bool TryGet(string id, out long amount) => _balances.TryGetValue(id, out amount);

        public void Set(string id, long amount) => _balances[id] = amount;
    }
}
