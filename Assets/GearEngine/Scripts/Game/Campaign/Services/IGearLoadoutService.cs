using GearEngine.GearEngine;

namespace GearEngine.Campaign.Services
{
    public interface IGearLoadoutService
    {
        bool HasSavedLoadout { get; }

        BoardLayoutData GetBoardLayout();

        void SaveBoardLayout(BoardLayoutData layout);
    }
}
