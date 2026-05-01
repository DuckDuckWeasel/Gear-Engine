using Scaffold.LiveOps.Authoring;
using UnityEngine;
using PerkConfig = LiveOps.Modules.DTO.Perks.PerkConfig;

namespace GearEngine.Perks.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Perk Config Builder", fileName = "PerkConfigBuilder")]
    public sealed class PerkConfigBuilderSO : ConfigBuilderSO<PerkConfig>
    {
        [Header("Asset source")]
        [SerializeField]
        private Config.PerkCatalogSO catalog;

        [Header("Asset-independent fields")]
        [SerializeField]
        private long baseCost = 100;

        [SerializeField]
        private long costPerPurchaseGrowth = 50;

        public override string ConfigKey => nameof(PerkConfig);

        public override PerkConfig Build()
        {
            var cfg = new PerkConfig
            {
                BaseCost = baseCost,
                CostPerPurchaseGrowth = costPerPurchaseGrowth,
            };

            if (catalog == null)
            {
                return cfg;
            }

            foreach (global::GearEngine.Perks.Config.PerkItem perk in catalog.All)
            {
                if (perk != null && !string.IsNullOrEmpty(perk.Id))
                {
                    cfg.Catalog.Add(perk.Id);
                }
            }

            return cfg;
        }

        public override void Apply(PerkConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseCost = pulled.BaseCost;
            costPerPurchaseGrowth = pulled.CostPerPurchaseGrowth;
        }
    }
}
