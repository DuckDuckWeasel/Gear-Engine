using UnityEngine;

namespace GearEngine.Cards.Powerups
{
    public abstract class CarPowerupModifierSO : ScriptableObject, ICarPowerupModifier
    {
        public CarPowerupApplyPhase Phase => phase;

        [SerializeField] private CarPowerupApplyPhase phase = CarPowerupApplyPhase.Multiplicative;

        public abstract void Apply(ref CarPowerupStats stats);
    }
}
