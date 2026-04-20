using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Tracks
{
    public sealed class TrackPersistence
    {
        [JsonProperty("currentTrackId")]
        public string CurrentTrackId { get; set; } = string.Empty;

        [JsonProperty("bestScores")]
        public Dictionary<string, int> BestScores { get; set; } = new Dictionary<string, int>();
    }
}
