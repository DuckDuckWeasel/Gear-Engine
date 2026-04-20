using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Tracks
{
    public sealed class TrackConfigEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("baseReward")]
        public int BaseReward { get; set; }

        [JsonProperty("bands")]
        public List<TrackScoreBandConfig> Bands { get; set; } = new List<TrackScoreBandConfig>();
    }
}
