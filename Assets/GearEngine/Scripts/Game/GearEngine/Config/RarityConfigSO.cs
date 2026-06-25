using UnityEngine;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "RarityConfig", menuName = "GearEngine/Config/Rarity Config", order = 0)]
    public class RarityConfigSO : ScriptableObject
    {
        public ItemRarity Rarity;
        public string DisplayName;
        public Color Color = Color.white;
        public Color TextColor = Color.white;
        public Sprite CardSprite;
    }
}
