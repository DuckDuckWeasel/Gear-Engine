using GameModuleDTO.Modules.Loadout;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Loadout Config Builder", fileName = "LoadoutConfigBuilder")]
    public sealed class LoadoutConfigBuilderSO : ConfigBuilderSO<LoadoutConfig>
    {
        [SerializeField]
        private int baseSlots = 6;

        public override string ConfigKey => nameof(LoadoutConfig);

        public override LoadoutConfig Build()
        {
            return new LoadoutConfig
            {
                BaseSlots = baseSlots,
            };
        }

        public override void Apply(LoadoutConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseSlots = pulled.BaseSlots;
        }
    }
}
