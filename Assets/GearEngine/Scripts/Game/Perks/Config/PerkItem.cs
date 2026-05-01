using System.Collections.Generic;
using GearEngine.Perks.Powerups;
using UnityEngine;

namespace GearEngine.Perks.Config
{
    [CreateAssetMenu(fileName = "PerkItem", menuName = "GearEngine/Perks/Perk Config")]
    public class PerkItem : ScriptableObject
    {
        public string Id => data?.Id;
        public PerkItemData Data => data;

        [SerializeField] private PerkItemData data;

        public IReadOnlyList<CarPowerupModifierSO> ModifierAssets => data?.ModifierAssets;

        public void CollectModifiers(List<ICarPowerupModifier> destination)
        {
            data?.CollectModifiers(destination);
        }
    }
}
