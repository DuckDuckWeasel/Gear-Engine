using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "BrakeToBurnGear", menuName = "Gear Engine/Abilities/Group E/Brake To Burn Gear")]
    public sealed class BrakeToBurnGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private float speedThreshold = 45f;
        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if(RaceContext != null && RaceContext.CurrentSpeed < speedThreshold) {
                // Instantly charge node if braking
                // (Visual hook placeholder)
            }
        }
    }
}
