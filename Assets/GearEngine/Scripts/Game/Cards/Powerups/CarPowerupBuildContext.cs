using System;
using System.Collections.Generic;

namespace GearEngine.Cards.Powerups
{
    /// <summary>sample: Resolved modifier stack for a single race / car setup. Built from collected card ids.</summary>
    public sealed class CarPowerupBuildContext
    {
        public CarPowerupBuildContext(IReadOnlyList<ICarPowerupModifier> modifiers)
        {
            this.modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        public IReadOnlyList<ICarPowerupModifier> Modifiers => modifiers;

        private readonly IReadOnlyList<ICarPowerupModifier> modifiers;

        public CarPowerupStats Evaluate()
        {
            var stats = CarPowerupStats.Neutral;
            for (var i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].Apply(ref stats);
            }

            return stats;
        }
    }
}
