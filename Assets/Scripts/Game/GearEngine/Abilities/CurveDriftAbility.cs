using UnityEngine;

namespace Scaffold.GearEngine.Abilities
{
    [CreateAssetMenu(fileName = "CurveDriftAbility", menuName = "GearEngine/Abilities/CurveDrift")]
    public class CurveDriftAbility : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            Debug.Log($"[CurveDriftAbility] Gear at {owner.Position} triggered Curve/Drift modification!");
        }
    }
}
