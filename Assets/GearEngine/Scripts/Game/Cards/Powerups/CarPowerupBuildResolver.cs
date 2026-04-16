using System;
using System.Collections.Generic;

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
            foreach (string id in collectedCardIds)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!definitions.TryGet(id, out CardDefinition card) || card == null)
                {
                    continue;
                }

                card.CollectModifiers(list);
            }

            list.Sort(CompareModifierOrder);
            return new CarPowerupBuildContext(list);
        }

        private static int CompareModifierOrder(ICarPowerupModifier a, ICarPowerupModifier b)
        {
            int byPhase = a.Phase.CompareTo(b.Phase);
            return byPhase;
        }
    }
}
