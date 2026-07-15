using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "GearItem", menuName = "GearEngine/Gear/Gear Item")]
    public class GearItem : ScriptableObject
    {
        public GearItem NextLevel => nextLevel;

        [Tooltip("Strong reference to the ScriptableObject for the next level upgrade.")]
        [SerializeField] private GearItem nextLevel;

        public string Id => data?.Id;

        [SerializeField] private GearItemData data;

        [Tooltip("Abilities that this gear executes when triggered.")]
        [SerializeField] private List<GearAbilitySO> abilities;

        public GearItemData CreateRuntimeData()
        {
            GearItemData runtime = data.Clone(nextLevel, abilities);
            runtime.SourceGearConfig = this;
            return runtime;
        }
    }
}
