using System;
using GearEngine.GearEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class LocalGearLoadoutService : IGearLoadoutService
    {
        private BoardLayoutData current;

        public bool HasSavedLoadout => current != null;

        public BoardLayoutData GetBoardLayout() => current;

        public void SaveBoardLayout(BoardLayoutData layout)
        {
            current = layout ?? throw new ArgumentNullException(nameof(layout));
        }
    }
}
