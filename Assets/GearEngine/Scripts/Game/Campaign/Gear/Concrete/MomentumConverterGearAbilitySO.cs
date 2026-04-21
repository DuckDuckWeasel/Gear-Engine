using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "MomentumConverterGear", menuName = "Gear Engine/Abilities/Group C/Momentum Converter Gear")]
    public sealed class MomentumConverterGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO penaltyStat;
        [SerializeField] private VariableSO bonusStat;
        private float currentBonus = 0f;
        private float tickTime = 0f;

        [SerializeField] private float conversionThreshold = 10f;
        [SerializeField] private float bonusIncrement = 5f;
        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null) return;
            tickTime += deltaTime;
            if(tickTime > conversionThreshold) {
                tickTime = 0f;
                currentBonus += bonusIncrement;
                ApplyModifier(owner, bonusStat, currentBonus);
                ApplyModifier(owner, penaltyStat, -currentBonus);
            }
        }
    }
}
