using GameModuleDTO.Modules.Inventory;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Inventory Config Builder", fileName = "InventoryConfigBuilder")]
    public sealed class InventoryConfigBuilderSO : ConfigBuilderSO<InventoryConfig>
    {
        [SerializeField]
        private int baseSlots = 8;

        [SerializeField]
        private string motorCogGearId = "gear_core";

        [SerializeField]
        private int motorCogStartX = 2;

        [SerializeField]
        private int motorCogStartY = 2;

        public override string ConfigKey => nameof(InventoryConfig);

        public override InventoryConfig Build()
        {
            return new InventoryConfig
            {
                BaseSlots = baseSlots,
                MotorCogGearId = motorCogGearId,
                MotorCogStartX = motorCogStartX,
                MotorCogStartY = motorCogStartY,
            };
        }

        public override void Apply(InventoryConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseSlots = pulled.BaseSlots;
            motorCogGearId = pulled.MotorCogGearId;
            motorCogStartX = pulled.MotorCogStartX;
            motorCogStartY = pulled.MotorCogStartY;
        }
    }
}
