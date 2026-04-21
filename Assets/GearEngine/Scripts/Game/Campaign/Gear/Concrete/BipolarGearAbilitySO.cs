using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "BipolarGear", menuName = "Gear Engine/Abilities/Group D/Bipolar Gear")]
    public sealed class BipolarGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO targ;
        private float tk = 0f;

        [SerializeField] private float stateSwitchInterval = 5f;
        [SerializeField] private float buffAmount = 50f;
        [SerializeField] private float debuffAmount = -30f;
        [SerializeField] private float effectDuration = 4f;
        public override void Tick(IGridNode owner, float deltaTime)
        {
            base.Tick(owner, deltaTime);
            if (RaceContext == null) return;
            tk += deltaTime;
            if(tk > stateSwitchInterval) {
                tk = 0f;
                bool good = Random.Range(0,100) > 50;
                ApplyModifier(owner, targ, good ? buffAmount : debuffAmount, effectDuration);
            }
        }
    }
}
