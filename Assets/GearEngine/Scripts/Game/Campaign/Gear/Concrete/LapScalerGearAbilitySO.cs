using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "LapScalerGear", menuName = "Gear Engine/Abilities/Group C/Lap Scaler Gear")]
    public sealed class LapScalerGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO stat;
        [SerializeField] private float baseBuff = 10f;

        [SerializeField] private float buffDuration = 5f;
        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;
            var lapMult = Mathf.Max(1, RaceContext.CurrentLap);
            ApplyModifier(owner, stat, baseBuff * lapMult, buffDuration);
        }
    }
}
