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
        }
    }
}
