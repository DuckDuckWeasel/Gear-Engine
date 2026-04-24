using LiveOps.Modules.DTO.Cards;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Cards.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Card Config Builder", fileName = "CardConfigBuilder")]
    public sealed class CardConfigBuilderSO : ConfigBuilderSO<CardConfig>
    {
        [Header("Asset source")]
        [SerializeField]
        private CardCatalogSO catalog;

        [Header("Asset-independent fields")]
        [SerializeField]
        private long baseCost = 100;

        [SerializeField]
        private long costPerPurchaseGrowth = 50;

        public override string ConfigKey => nameof(CardConfig);

        public override CardConfig Build()
        {
            var cfg = new CardConfig
            {
                BaseCost = baseCost,
                CostPerPurchaseGrowth = costPerPurchaseGrowth,
            };

            if (catalog == null)
            {
                return cfg;
            }

            foreach (CardDefinition card in catalog.Cards)
            {
                if (card != null && !string.IsNullOrEmpty(card.Id))
                {
                    cfg.Catalog.Add(card.Id);
                }
            }

            return cfg;
        }

        public override void Apply(CardConfig pulled)
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
