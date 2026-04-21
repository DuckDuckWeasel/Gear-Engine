using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "OverheatGear", menuName = "Gear Engine/Abilities/Group C/Overheat Gear")]
    public sealed class OverheatGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO speedStat;
        [SerializeField] private VariableSO brakeStat;
        
        [SerializeField] private float boostAmount = 200f;
        [SerializeField] private float boostDuration = 15f;
        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;
            ApplyModifier(owner, speedStat, boostAmount, boostDuration);
            // Wait, we don't have async/coroutines in SO. Let's just track it in Tick!
        }
        
        // Simple tick tracking for cooldown penalty
    }
}
