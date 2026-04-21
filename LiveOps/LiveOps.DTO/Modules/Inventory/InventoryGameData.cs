using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class InventoryGameData : IGameModuleData
    {
        public string Key => nameof(InventoryGameData);

        [JsonProperty("gears")]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();

        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; }

        [JsonProperty("motorCogGearId")]
        public string MotorCogGearId { get; set; } = string.Empty;

        [JsonProperty("motorCogStartX")]
        public int MotorCogStartX { get; set; } = 2;

        [JsonProperty("motorCogStartY")]
        public int MotorCogStartY { get; set; } = 2;

        [JsonConstructor]
        private InventoryGameData()
        {
        }

        public InventoryGameData(InventoryPersistence persistence, InventoryConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Gears = new List<OwnedGearEntry>(persistence.Gears);
            BaseSlots = config.BaseSlots;
            MotorCogGearId = config.MotorCogGearId ?? string.Empty;
            MotorCogStartX = config.MotorCogStartX;
            MotorCogStartY = config.MotorCogStartY;
        }
    }
}
