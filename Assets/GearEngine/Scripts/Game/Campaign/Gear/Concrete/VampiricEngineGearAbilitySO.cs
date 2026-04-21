using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "VampiricEngineGear", menuName = "Gear Engine/Abilities/Group C/Vampiric Engine Gear")]
    public sealed class VampiricEngineGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO targetStat;
        private float currentStack = 0f;

        [SerializeField] private float stackIncreaseVal = 2f;
        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null) return;
            currentStack += stackIncreaseVal;
            // Permanent stack
            ApplyModifier(owner, targetStat, currentStack);
        }
    }
}
