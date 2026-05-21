using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    public enum ItemScreenType
    {
        Perks,
        Gears
    }

    [CreateAssetMenu(fileName = "ItemsScreenState", menuName = "GearEngine/Campaign/ItemsScreenState")]
    public class ItemsScreenState : ScriptableObject
    {
        public ItemScreenType TypeToDisplay;
        public bool ShowBuyButton = true;
        public bool ShowUnownedItems = true;
    }
}
