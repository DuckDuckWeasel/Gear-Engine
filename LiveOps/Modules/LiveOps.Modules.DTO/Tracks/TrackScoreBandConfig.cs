using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Tracks
{
    public sealed class TrackScoreBandConfig
    {
        [JsonProperty("maxSec")]
        public float MaxRaceTimeSeconds { get; set; }

        [JsonProperty("r")]
        public int Reward { get; set; }
    }
}
