using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.Campaign.Gear
{
    [CreateAssetMenu(fileName = "TheJokerGear", menuName = "Gear Engine/Abilities/Group D/The Joker Gear")]
    public sealed class TheJokerGearAbilitySO : ActiveRaceGearAbilitySO
    {
        [SerializeField] private VariableSO s1;
        [SerializeField] private VariableSO s2;
        private bool flipped = false;

        [SerializeField] private float buffAmount = 300f;
        [SerializeField] private float debuffAmount = -150f;

        public override void Execute(IGridNode owner)
        {
            if (RaceContext == null || flipped) return;
            flipped = true;
            ApplyModifier(owner, s1, buffAmount);
            ApplyModifier(owner, s2, debuffAmount);
        }
    }
}
