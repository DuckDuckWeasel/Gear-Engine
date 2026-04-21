using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "MirageGear", menuName = "Gear Engine/Abilities/Group D/Mirage Gear")]
    public sealed class MirageGearAbilitySO : ActiveRaceGearAbilitySO
    {
        private bool isBroken = false;
        private int lastLap = 0;

        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (isBroken || RaceContext == null) return;

            if(RaceContext.CurrentLap > lastLap) {
                lastLap = RaceContext.CurrentLap;
                if(Random.Range(0,100) < 25) {
                    isBroken = true;
                    Debug.Log("Mirage gear died!");
                }
            }
        }
    }
}
