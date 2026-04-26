using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Loadout
{
    [LiveOpsKey(nameof(LoadoutGameData))]
    public sealed class LoadoutGameData : IGameModuleData
    {
        public string Key => nameof(LoadoutGameData);

        [JsonProperty("board")]
        public List<LoadoutPlacement> Board { get; set; } = new List<LoadoutPlacement>();

        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; }

        [JsonProperty("motorCogStartX")]
        public int MotorCogStartX { get; set; } = 2;

        [JsonProperty("motorCogStartY")]
        public int MotorCogStartY { get; set; } = 2;

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
            MotorCogStartX = config.MotorCogStartX;
            MotorCogStartY = config.MotorCogStartY;
        }
    }
}
