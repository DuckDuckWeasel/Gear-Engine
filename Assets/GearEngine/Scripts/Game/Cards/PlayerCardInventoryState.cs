using System;
using System.Collections.Generic;

namespace GearEngine.Cards
{
    /// <summary>sample: Client-side inventory — ordered slots plus collected card ids for powerup resolution.</summary>
    [Serializable]
    public sealed class PlayerCardInventoryState
    {
        public List<CardSlotSnapshot> Slots = new List<CardSlotSnapshot>();

        public IEnumerable<string> EnumerateCollectedCardIds()
        {
            for (var i = 0; i < Slots.Count; i++)
            {
                CardSlotSnapshot s = Slots[i];
                if (s.State == CardSlotState.Collected && !string.IsNullOrEmpty(s.CardId))
                {
                    yield return s.CardId;
                }
            }
        }
    }
}
