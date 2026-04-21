using GearEngine.GearEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalGearLoadoutService : IGearLoadoutService
    {
        private BoardLayoutData current;

        public bool HasSavedLoadout => current != null;

        public BoardLayoutData GetBoardLayout() => current;
    }
}
