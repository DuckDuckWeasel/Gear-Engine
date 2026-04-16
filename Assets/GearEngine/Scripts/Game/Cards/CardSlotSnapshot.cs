using System;

namespace GearEngine.Cards
{
    /// <summary>sample: Serializable slot row for sync with backend later (index, state, optional assigned card).</summary>
    [Serializable]
    public sealed class CardSlotSnapshot
    {
        public int SlotIndex;
        public CardSlotState State;
        public string CardId;
    }
}
