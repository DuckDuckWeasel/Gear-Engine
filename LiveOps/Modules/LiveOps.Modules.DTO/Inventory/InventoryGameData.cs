using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Inventory
{
    public sealed class InventoryGameData : IGameModuleData
    {
        public string Key => nameof(InventoryGameData);

        [JsonProperty("gears")]
        public List<OwnedGearEntry> Gears { get; set; } = new List<OwnedGearEntry>();

        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; }

        /// <summary>Derived from <c>InventoryConfig.StartingGearIds[0]</c> (core/motor catalog id).</summary>
        [JsonProperty("motorCogGearId")]
        public string MotorCogGearId { get; set; } = string.Empty;

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
            MotorCogGearId = config.GetCoreGearCatalogId();
        }
    }
}
