using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Loadout
{
    public sealed class LoadoutGameData : IGameModuleData
    {
        public string Key => nameof(LoadoutGameData);

        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();

        [JsonConstructor]
        private LoadoutGameData()
        {
        }

        public LoadoutGameData(LoadoutPersistence persistence)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            Board = new List<LoadoutPlacement>(persistence.Board);
        }
    }
}
