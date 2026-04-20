using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Tracks
{
    public sealed class TrackGameData : IGameModuleData
    {
        public string Key => nameof(TrackGameData);

        [JsonProperty("currentTrackId")]
        public string CurrentTrackId { get; set; } = string.Empty;

        [JsonProperty("orderedTrackIds")]
        public List<string> OrderedTrackIds { get; set; } = new List<string>();

        [JsonProperty("bestScores")]
        public Dictionary<string, int> BestScores { get; set; } = new Dictionary<string, int>();

        [JsonConstructor]
        private TrackGameData()
        {
        }

        public TrackGameData(TrackPersistence persistence, TrackConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            CurrentTrackId = persistence.CurrentTrackId;
            foreach (TrackConfigEntry e in config.Entries)
            {
                if (e != null && !string.IsNullOrEmpty(e.Id))
                {
                    OrderedTrackIds.Add(e.Id);
                }
            }

            BestScores = new Dictionary<string, int>(persistence.BestScores);
        }
    }
}
