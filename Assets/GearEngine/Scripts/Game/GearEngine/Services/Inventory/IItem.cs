namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IItem
    {
        string Id { get; }
        ItemRarity Rarity { get; }
        string Description { get; }
    }
}
