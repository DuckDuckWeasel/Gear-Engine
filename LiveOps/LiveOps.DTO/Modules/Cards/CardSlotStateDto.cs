namespace GameModuleDTO.Modules.Cards
{
    /// <summary>
    /// Slot lifecycle for card collection. Values align with client <c>GearEngine.Cards.CardSlotState</c>.
    /// </summary>
    public enum CardSlotStateDto
    {
        Blocked = 0,
        Uncollected = 1,
        Collected = 2,
    }
}
