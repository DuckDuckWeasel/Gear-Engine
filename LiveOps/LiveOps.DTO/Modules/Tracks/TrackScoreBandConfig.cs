using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Tracks
{
    public sealed class TrackScoreBandConfig
    {
        [JsonProperty("maxSec")]
        public float MaxRaceTimeSeconds { get; set; }

        [JsonProperty("r")]
        public int Reward { get; set; }
    }
}
