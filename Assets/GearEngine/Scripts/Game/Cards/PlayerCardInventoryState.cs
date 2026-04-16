using System;
using System.Collections.Generic;

namespace GearEngine.Cards
{
    /// <summary>
    /// Client-side inventory: ordered slots plus collected card ids for powerup resolution.
    /// </summary>
    [Serializable]
    public sealed class PlayerCardInventoryState
    {
        public List<CardSlotSnapshot> slots = new List<CardSlotSnapshot>();

        public IEnumerable<string> EnumerateCollectedCardIds()
        {
            for (var i = 0; i < slots.Count; i++)
            {
                CardSlotSnapshot s = slots[i];
                if (s.state == CardSlotState.Collected && !string.IsNullOrEmpty(s.cardId))
                {
                    yield return s.cardId;
                }
            }
        }
    }
}
