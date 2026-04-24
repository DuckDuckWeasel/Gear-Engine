using GearEngine.GearEngine;

namespace GearEngine.Campaign.Services
{
    public interface IGearLoadoutService
    {
        bool HasSavedLoadout { get; }

        int BoardSlotCapacity { get; }

        BoardLayoutData GetBoardLayout();
    }
}
