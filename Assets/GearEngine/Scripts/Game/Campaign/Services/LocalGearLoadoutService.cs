using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalGearLoadoutService : IGearLoadoutService
    {
        private BoardLayoutData current;
        private List<GearConfig> inventoryGearConfigs;

        public bool HasSavedLoadout => current != null;

        public BoardLayoutData GetBoardLayout() => current;

        public void SaveBoardLayout(BoardLayoutData layout)
        {
            current = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        public bool HasSavedInventory => inventoryGearConfigs != null;

        public IReadOnlyList<GearConfig> GetInventoryGearConfigs() => inventoryGearConfigs;

        public void SaveInventoryGearConfigs(IReadOnlyList<GearConfig> gears)
        {
            if (gears == null)
            {
                throw new ArgumentNullException(nameof(gears));
            }

            inventoryGearConfigs = new List<GearConfig>(gears);
        }
    }
}
