using System.Collections.Generic;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Player-owned gear list persisted via LiveOps (not in-race <see cref="IRaceInventoryService"/>).
    /// </summary>
    public interface IOwnedGearInventoryService
    {
        bool HasSavedInventory { get; }

        IReadOnlyList<GearConfig> GetOwnedGearConfigs();
    }
}
