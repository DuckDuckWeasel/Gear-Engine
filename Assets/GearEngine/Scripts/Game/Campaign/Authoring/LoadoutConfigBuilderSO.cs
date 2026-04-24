using LiveOps.Modules.DTO.Loadout;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Loadout Config Builder", fileName = "LoadoutConfigBuilder")]
    public sealed class LoadoutConfigBuilderSO : ConfigBuilderSO<LoadoutConfig>
    {
        [SerializeField]
        private int baseSlots = 6;

        [SerializeField]
        private int motorCogStartX = 2;

        [SerializeField]
        private int motorCogStartY = 2;

        public override string ConfigKey => nameof(LoadoutConfig);

        public override LoadoutConfig Build()
        {
            return new LoadoutConfig
            {
                BaseSlots = baseSlots,
                MotorCogStartX = motorCogStartX,
                MotorCogStartY = motorCogStartY,
            };
        }

        public override void Apply(LoadoutConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseSlots = pulled.BaseSlots;
            motorCogStartX = pulled.MotorCogStartX;
            motorCogStartY = pulled.MotorCogStartY;
        }
    }
}
