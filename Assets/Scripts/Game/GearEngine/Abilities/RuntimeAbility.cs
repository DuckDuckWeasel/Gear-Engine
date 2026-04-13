using UnityEngine;

namespace GearEngine.GearEngine.Abilities
{
    public class RuntimeAbility
    {
        public GearAbilitySO AbilityDef { get; private set; }
        public float DurationRemaining { get; set; }
        public bool IsPermanent => DurationRemaining < 0;

        public RuntimeAbility(GearAbilitySO abilityDef, float durationRemaining)
        {
            AbilityDef = abilityDef;
            DurationRemaining = durationRemaining;
        }
    }
}
