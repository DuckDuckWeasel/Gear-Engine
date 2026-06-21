namespace GearEngine.GearEngine.Services.Inventory
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public static class ItemRarityExtensions
    {
        public static string GetColorHex(this ItemRarity r) => r switch
        {
            ItemRarity.Common => "AAAAAA",
            ItemRarity.Uncommon => "1EFF00",
            ItemRarity.Rare => "0070FF",
            ItemRarity.Epic => "A335EE",
            ItemRarity.Legendary => "FF8000",
            _ => "FFFFFF"
        };
    }
}
