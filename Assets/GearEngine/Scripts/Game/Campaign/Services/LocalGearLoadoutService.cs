using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalGearLoadoutService : IGearLoadoutService, IBoardSlotCapacityProvider
    {
        public LocalGearLoadoutService(int boardSlotCapacity = int.MaxValue)
        {
            this.boardSlotCapacity = boardSlotCapacity;
        }

        public bool HasSavedLoadout => current != null;

        private BoardLayoutData current;

        public int BoardSlotCapacity => boardSlotCapacity;

        private readonly int boardSlotCapacity;

        public BoardLayoutData GetBoardLayout()
        {
            return current;
        }
    }
}
