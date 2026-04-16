using UnityEngine;

namespace GearEngine.Cards.Powerups
{
    public abstract class CarPowerupModifierSO : ScriptableObject, ICarPowerupModifier
    {
        [SerializeField] private CarPowerupApplyPhase phase = CarPowerupApplyPhase.Multiplicative;

        public CarPowerupApplyPhase Phase => phase;

        public abstract void Apply(ref CarPowerupStats stats);
    }
}
