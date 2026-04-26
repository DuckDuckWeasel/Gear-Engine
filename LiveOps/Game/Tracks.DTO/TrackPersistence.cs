using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Tracks
{
    [LiveOpsKey(nameof(TrackPersistence))]
    public sealed class TrackPersistence
    {
        [JsonProperty("currentTrackId")]
        public string CurrentTrackId { get; set; } = string.Empty;

        [JsonProperty("bestTimeSec")]
        public Dictionary<string, float> BestTimeSec { get; set; } = new Dictionary<string, float>();
    }
}
