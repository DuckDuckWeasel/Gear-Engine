using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.Cards
{
    [CreateAssetMenu(menuName = "Game/Cards/Card Catalog", fileName = "CardCatalog")]
    public sealed class CardCatalogSO : ScriptableObject, ICardDefinitionProvider
    {
        [SerializeField] private List<CardDefinition> cards = new List<CardDefinition>();

        public IReadOnlyList<CardDefinition> Cards => cards;

        public bool TryGet(string cardId, out CardDefinition definition)
        {
            definition = null;
            if (string.IsNullOrEmpty(cardId))
            {
                return false;
            }

            for (var i = 0; i < cards.Count; i++)
            {
                CardDefinition c = cards[i];
                if (c != null && string.Equals(c.Id, cardId, StringComparison.Ordinal))
                {
                    definition = c;
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<CardDefinition> GetRollPool()
        {
            return cards;
        }
    }
}
