using UnityEngine;

namespace GearEngine.Perks.Powerups
{
    public abstract class CarPowerupModifierSO : ScriptableObject, ICarPowerupModifier
    {
        public CarPowerupApplyPhase Phase => phase;

        [SerializeField] private CarPowerupApplyPhase phase = CarPowerupApplyPhase.Multiplicative;

        public abstract void Apply(ref CarPowerupStats stats);

        public virtual string GetFormattedValue() => string.Empty;

        protected string FormatValueColored(float val, string suffix = "")
        {
            if (val > 0) return $"<color=#1EFF00><i>+{val:0.##}{suffix}</i></color>";
            if (val < 0) return $"<color=#FF0000><i>{val:0.##}{suffix}</i></color>";
            return $"{val:0.##}{suffix}";
        }
    }
}
