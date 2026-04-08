using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(fileName = "GearConfig", menuName = "GearEngine/GearConfig")]
    public class GearConfig : ScriptableObject
    {
        [SerializeField] private GearConfigData data;
        
        [Tooltip("Strong reference to the ScriptableObject for the next level upgrade.")]
        [SerializeField] private GearConfig nextLevel;

        [Tooltip("Abilities that this gear executes when triggered.")]
        [SerializeField] private List<GearAbilitySO> abilities;

        public GearConfig NextLevel => nextLevel;
        public string Id => data?.Id;

        public GearConfigData CreateRuntimeData()
        {
            return data.Clone(nextLevel, abilities);
        }
    }
}
