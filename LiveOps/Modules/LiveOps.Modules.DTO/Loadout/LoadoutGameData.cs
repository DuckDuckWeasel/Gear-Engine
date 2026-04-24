using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Loadout
{
    public sealed class LoadoutGameData : IGameModuleData
    {
        public string Key => nameof(LoadoutGameData);

        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();

        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; }

        [JsonConstructor]
        private LoadoutGameData()
        {
        }

        public LoadoutGameData(LoadoutPersistence persistence, LoadoutConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Board = new List<LoadoutPlacement>(persistence.Board);
            BaseSlots = config.BaseSlots;
        }
    }
}
