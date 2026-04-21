using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "PacemakerGear", menuName = "Gear Engine/Abilities/Group E/Pacemaker Gear")]
    public sealed class PacemakerGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO stat;

        [SerializeField] private float minSpeedThreshold = 40f;
        [SerializeField] private float maxSpeedThreshold = 60f;
        [SerializeField] private float boostAmount = 100f;
        [SerializeField] private float boostDuration = 3f;
        public override void Execute(IGridNode owner)
        {
            if(RaceContext != null && RaceContext.CurrentSpeed > minSpeedThreshold && RaceContext.CurrentSpeed < maxSpeedThreshold) {
                ApplyModifier(owner, stat, boostAmount, boostDuration);
            }
        }
    }
}
