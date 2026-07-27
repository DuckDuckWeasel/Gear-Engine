using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "RadioactiveEngineGear", menuName = "GearEngine/Abilities/Group C/Radioactive Engine Gear")]
    public sealed class RadioactiveEngineGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO decayTarget;
        [SerializeField] private float decayAmount = 1f;
        private float decayStack = 0f;
        private float tickT = 0f;

        [SerializeField] private float passiveTickRate = 2f;
        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null) return;
            tickT += deltaTime;
            if(tickT > passiveTickRate) {
                tickT = 0f;
                decayStack -= decayAmount;
                ApplyModifier(owner, decayTarget, decayStack);
            }
        }
    }
}
