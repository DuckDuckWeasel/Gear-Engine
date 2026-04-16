using System;

namespace GearEngine.Cards
{
    /// <summary>
    /// Serializable slot row for sync with backend later (id + state + optional assigned card).
    /// </summary>
    [Serializable]
    public sealed class CardSlotSnapshot
    {
        public int slotIndex;
        public CardSlotState state;
        public string cardId;
    }
}
