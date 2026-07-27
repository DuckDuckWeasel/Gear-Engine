using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "SlotMachineGear", menuName = "GearEngine/Abilities/Group D/Slot Machine Gear")]
    public sealed class SlotMachineGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO stat;

        [SerializeField] private float jackpotBonus = 999f;
        [SerializeField] private float minorBonus = 5f;
        [SerializeField] private float minorDuration = 2f;
        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;
            bool jackpot = Random.Range(0, 100) > 95;
            if(jackpot) ApplyModifier(owner, stat, jackpotBonus);
            else ApplyModifier(owner, stat, minorBonus, minorDuration);
        }
    }
}
