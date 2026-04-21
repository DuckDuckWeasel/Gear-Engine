using System;
using System.Collections.Generic;
using GameModuleDTO.GameModule;
using Newtonsoft.Json;

namespace GameModuleDTO.Modules.Inventory
{
    public sealed class InventoryGameData : IGameModuleData
    {
        public string Key => nameof(InventoryGameData);

        [JsonProperty("gearIds")]
        public List<string> GearIds { get; set; } = new List<string>();

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

            GearIds = new List<string>(persistence.GearIds);
            BaseSlots = config.BaseSlots;
        }
    }
}
