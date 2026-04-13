using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    public class RuntimeAbility
    {
        public RuntimeAbility(GearAbilitySO abilityDef, float durationRemaining)
        {
            AbilityDef = abilityDef;
            DurationRemaining = durationRemaining;
        }

        public GearAbilitySO AbilityDef { get; private set; }
        public float DurationRemaining { get; set; }
        public bool IsPermanent => DurationRemaining < 0;
    }
}
