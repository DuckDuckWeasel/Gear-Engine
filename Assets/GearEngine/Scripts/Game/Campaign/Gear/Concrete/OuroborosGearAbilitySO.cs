using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "OuroborosGear", menuName = "Gear Engine/Abilities/Group C/Ouroboros Gear")]
    public sealed class OuroborosGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private List<VariableSO> cycleStats;
        [SerializeField] private float buffVal = 50f;
        private int currentIndex = 0;

        [SerializeField] private float buffDuration = 6f;
        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null || cycleStats == null || cycleStats.Count == 0) return;
            var t = cycleStats[currentIndex];
            ApplyModifier(owner, t, buffVal, buffDuration);
            currentIndex = (currentIndex + 1) % cycleStats.Count;
        }
    }
}
