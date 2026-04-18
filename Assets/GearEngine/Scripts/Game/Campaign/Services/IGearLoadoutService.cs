using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Services
{
    public interface IGearLoadoutService
    {
        bool HasSavedLoadout { get; }

        BoardLayoutData GetBoardLayout();

        void SaveBoardLayout(BoardLayoutData layout);

        bool HasSavedInventory { get; }

        IReadOnlyList<GearConfig> GetInventoryGearConfigs();

        void SaveInventoryGearConfigs(IReadOnlyList<GearConfig> gears);
    }
}
