using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Currency
{
    public sealed class CurrencyConfig
    {
        [JsonProperty("entries")]
        private List<CurrencyConfigEntry> _entries = new List<CurrencyConfigEntry>();

        [JsonIgnore]
        public IReadOnlyList<CurrencyConfigEntry> Entries => _entries;

        public bool TryGet(string id, out CurrencyConfigEntry entry)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null && _entries[i].Id == id)
                {
                    entry = _entries[i];
                    return true;
                }
            }

            entry = null;
            return false;
        }
    }
}
