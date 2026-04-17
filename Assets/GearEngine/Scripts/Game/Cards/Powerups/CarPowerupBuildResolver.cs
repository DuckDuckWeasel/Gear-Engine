using System;
using System.Collections.Generic;
using GearEngine.Cards;

namespace GearEngine.Cards.Powerups
{
    public sealed class CarPowerupBuildResolver
    {
        public CarPowerupBuildResolver(ICardDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        private readonly ICardDefinitionProvider definitions;

        public CarPowerupBuildContext Resolve(IEnumerable<string> collectedCardIds)
        {
            if (collectedCardIds == null)
            {
                throw new ArgumentNullException(nameof(collectedCardIds));
            }

            var list = new List<ICarPowerupModifier>(16);
            AppendModifiersFromIds(collectedCardIds, list);
            list.Sort(CompareModifierOrder);
            return new CarPowerupBuildContext(list);
        }

        private void AppendModifiersFromIds(IEnumerable<string> collectedCardIds, List<ICarPowerupModifier> list)
        {
            foreach (string id in collectedCardIds)
            {
                TryAppendModifiersForId(id, list);
            }
        }

        private void TryAppendModifiersForId(string id, List<ICarPowerupModifier> list)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            if (!definitions.TryGet(id, out CardDefinition card) || card == null)
            {
                return;
            }

            card.CollectModifiers(list);
        }

        private int CompareModifierOrder(ICarPowerupModifier a, ICarPowerupModifier b)
        {
            return a.Phase.CompareTo(b.Phase);
        }
    }
}
