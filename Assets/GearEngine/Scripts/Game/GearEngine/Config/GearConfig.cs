using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "GearConfig", menuName = "GearEngine/GearConfig")]
    public class GearConfig : ScriptableObject
    {
        public GearConfig NextLevel => nextLevel;

        [Tooltip("Strong reference to the ScriptableObject for the next level upgrade.")]
        [SerializeField] private GearConfig nextLevel;

        public string Id => data?.Id;

        [SerializeField] private GearConfigData data;

        [Tooltip("Abilities that this gear executes when triggered.")]
        [SerializeField] private List<GearAbilitySO> abilities;

        public GearConfigData CreateRuntimeData()
        {
            GearConfigData runtime = data.Clone(nextLevel, abilities);
            runtime.SourceGearConfig = this;
            return runtime;
        }
    }
}
