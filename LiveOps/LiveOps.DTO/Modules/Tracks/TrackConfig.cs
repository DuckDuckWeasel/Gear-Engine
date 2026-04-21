using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Tracks
{
    public sealed class TrackConfig
    {
        [JsonProperty("entries")]
        private List<TrackConfigEntry> _entries = new List<TrackConfigEntry>();

        [JsonIgnore]
        public IReadOnlyList<TrackConfigEntry> Entries => _entries;

        public void Clear() => _entries.Clear();

        public void AddEntry(TrackConfigEntry entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }

        public bool TryGet(string id, out TrackConfigEntry entry)
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
