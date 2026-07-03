using GearEngine.GearEngine.Services.Inventory;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    /// <summary>
    /// Static single source of truth for Rarity colors across the game.
    /// Values are extracted from the actual RarityConfigSO assets used in cards.
    /// </summary>
    public static class RarityPalette
    {
        public static readonly Color CommonColor = new Color(0.6666667f, 0.6666667f, 0.6666667f, 1f); // Gray
        public static readonly Color UncommonColor = new Color(0.133333f, 0.733333f, 0.133333f, 1f); // Green (Standard RPG)
        public static readonly Color RareColor = new Color(0f, 0.43922f, 1f, 1f); // Blue
        public static readonly Color EpicColor = new Color(0.63922f, 0.20784f, 0.93333f, 1f); // Purple
        public static readonly Color LegendaryColor = new Color(1f, 0.50196f, 0f, 1f); // Orange

        public static Color GetColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => CommonColor,
                ItemRarity.Uncommon => UncommonColor,
                ItemRarity.Rare => RareColor,
                ItemRarity.Epic => EpicColor,
                ItemRarity.Legendary => LegendaryColor,
                _ => Color.white
            };
        }

        public static Color GetColorByTier(int tier)
        {
            // Tiers: 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
            int maxTier = System.Enum.GetValues(typeof(ItemRarity)).Length - 1;
            int clampedTier = Mathf.Clamp(tier, 0, maxTier);
            return GetColor((ItemRarity)clampedTier);
        }
    }
}
