using System.Collections.Generic;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Currency
{
    /// <summary>
    /// Remote Config wire shape (numeric entries only). In Unity, use the Currency Config Builder with CurrencySO assets.
    /// </summary>
    public sealed class CurrencyConfig
    {
        [JsonProperty("entries")]
        private List<CurrencyConfigEntry> _entries = new List<CurrencyConfigEntry>();

        [JsonIgnore]
        public IReadOnlyList<CurrencyConfigEntry> Entries => _entries;

        public void Clear() => _entries.Clear();

        public void AddEntry(CurrencyConfigEntry entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }

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
