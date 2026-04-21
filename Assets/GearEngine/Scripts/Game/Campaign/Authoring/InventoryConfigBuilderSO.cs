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

        public override string ConfigKey => nameof(InventoryConfig);

        public override InventoryConfig Build()
        {
            return new InventoryConfig
            {
                BaseSlots = baseSlots,
            };
        }

        public override void Apply(InventoryConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseSlots = pulled.BaseSlots;
        }
    }
}
