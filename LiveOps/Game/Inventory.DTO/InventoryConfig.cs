using System.Collections.Generic;
using Newtonsoft.Json;
using LiveOps.DTO.Keys;

namespace LiveOps.Modules.DTO.Inventory
{
    /// <summary>
    /// Remote Config / Cloud Code wire shape (plain string ids). In Unity, use the Inventory Config Builder asset
    /// (GearConfig references); its Build output populates this DTO for sync/deploy.
    /// </summary>
    [LiveOpsKey(nameof(InventoryConfig))]
    public sealed class InventoryConfig
    {
        [JsonProperty("baseSlots")]
        public int BaseSlots { get; set; } = 8;

        /// <summary>
        /// Ordered catalog ids for brand-new players. Index <c>0</c> is the core/motor gear id.
        /// Seeded once (gated by <see cref="InventoryPersistence.StartingGearsSeeded"/>).
        /// </summary>
        [JsonProperty("startingGearIds")]
        public List<string> StartingGearIds { get; set; } = new List<string>();

        /// <summary>Core/motor catalog id from remote config; same as <c>StartingGearIds[0]</c> when present.</summary>
        public string GetCoreGearCatalogId()
        {
            if (StartingGearIds == null || StartingGearIds.Count == 0)
            {
                return string.Empty;
            }

            return StartingGearIds[0] ?? string.Empty;
        }
    }
}
