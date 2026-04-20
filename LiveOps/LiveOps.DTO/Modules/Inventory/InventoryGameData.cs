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

        [JsonConstructor]
        private InventoryGameData()
        {
        }

        public InventoryGameData(InventoryPersistence persistence)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            GearIds = new List<string>(persistence.GearIds);
        }
    }
}
