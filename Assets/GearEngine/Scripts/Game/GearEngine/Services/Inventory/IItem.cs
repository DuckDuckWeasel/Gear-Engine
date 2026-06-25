using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine.Services.Inventory
{
    public interface IItem
    {
        string Id { get; }
        string Name { get; }
        ItemRarity Rarity { get; }
        RarityConfigSO RarityConfig { get; }
        string Description { get; }
        UnityEngine.Sprite Icon { get; }
    }
}
