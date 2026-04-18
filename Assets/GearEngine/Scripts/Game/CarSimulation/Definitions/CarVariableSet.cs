using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        [FormerlySerializedAs("maxStraightSpeed")]
        [SerializeField] private VariableSO speed;
        [SerializeField] private VariableSO acceleration;
        [SerializeField] private VariableSO handling;
        [SerializeField] private VariableSO stability;
        [SerializeField] private VariableSO recovery;
        [SerializeField] private VariableSO driftPenalty;

        public VariableSO Speed => speed;
        public VariableSO Acceleration => acceleration;
        public VariableSO Handling => handling;
        public VariableSO Stability => stability;
        public VariableSO Recovery => recovery;
        public VariableSO DriftPenalty => driftPenalty;

        internal void AssignVariablesForTests(VariableSO speedVariable)
        {
            speed = speedVariable;
        }
    }
}
