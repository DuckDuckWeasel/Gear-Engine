using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        public VariableSO Speed => speed;
        public VariableSO Acceleration => acceleration;
        public VariableSO Brake => brake;
        public VariableSO Handling => handling;
        public VariableSO Stability => stability;
        public VariableSO Recovery => recovery;
        public VariableSO DriftPenalty => driftPenalty;

        [SerializeField] private VariableSO speed;
        [SerializeField] private VariableSO acceleration;
        [SerializeField] private VariableSO brake;
        [SerializeField] private VariableSO handling;
        [SerializeField] private VariableSO stability;
        [SerializeField] private VariableSO recovery;
        [SerializeField] private VariableSO driftPenalty;
    }
}
