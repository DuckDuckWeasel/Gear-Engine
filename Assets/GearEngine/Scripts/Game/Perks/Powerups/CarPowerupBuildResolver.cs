using System;
using System.Collections.Generic;
using GearEngine.Perks;
using GearEngine.Perks.Config;

namespace GearEngine.Perks.Powerups
{
    public sealed class CarPowerupBuildResolver
    {
        public CarPowerupBuildResolver(IPerkDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        private readonly IPerkDefinitionProvider definitions;

        public CarPowerupBuildContext Resolve(IEnumerable<string> collectedPerkIds)
        {
            if (collectedPerkIds == null)
            {
                throw new ArgumentNullException(nameof(collectedPerkIds));
            }

            var list = new List<ICarPowerupModifier>(16);
            AppendModifiersFromIds(collectedPerkIds, list);
            list.Sort(CompareModifierOrder);
            return new CarPowerupBuildContext(list);
        }

        private void AppendModifiersFromIds(IEnumerable<string> collectedPerkIds, List<ICarPowerupModifier> list)
        {
            foreach (string id in collectedPerkIds)
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

            if (!definitions.TryGet(id, out PerkItem perk) || perk == null)
            {
                return;
            }

            perk.CollectModifiers(list);
        }

        private int CompareModifierOrder(ICarPowerupModifier a, ICarPowerupModifier b)
        {
            return a.Phase.CompareTo(b.Phase);
        }
    }
}
