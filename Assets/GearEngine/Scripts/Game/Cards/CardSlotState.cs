namespace GearEngine.Cards
{
    /// <summary>
    /// Mirrors backend slot state: blocked progression, purchasable, or permanently collected.
    /// </summary>
    public enum CardSlotState
    {
        Blocked = 0,
        Uncollected = 1,
        Collected = 2,
    }
}
