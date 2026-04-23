using System.Collections.Generic;
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

        [SerializeField]
        [Tooltip("Catalog gear ids granted to brand-new players in addition to the motor cog. Seeded once per account.")]
        private List<string> startingGearIds = new List<string>
        {
            "gear_base_1",
            "gear_speed",
            "gear_score",
        };

        public override string ConfigKey => nameof(InventoryConfig);

        public override InventoryConfig Build()
        {
            return new InventoryConfig
            {
                BaseSlots = baseSlots,
                MotorCogGearId = motorCogGearId,
                MotorCogStartX = motorCogStartX,
                MotorCogStartY = motorCogStartY,
                StartingGearIds = startingGearIds != null ? new List<string>(startingGearIds) : new List<string>(),
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
            startingGearIds = pulled.StartingGearIds != null ? new List<string>(pulled.StartingGearIds) : new List<string>();
        }
    }
}
