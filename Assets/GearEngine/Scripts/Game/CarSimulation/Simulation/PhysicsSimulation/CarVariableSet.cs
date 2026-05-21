using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.CarSimulation.PhysicsSimulation
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        [SerializeField] private VariableSO speedCapability;
        [SerializeField] private VariableSO corneringSkill;
        [SerializeField] private VariableSO drift;
        [SerializeField] private VariableSO precision;
        [SerializeField] private VariableSO smoothness;

        public VariableSO SpeedCapability => speedCapability;
        public VariableSO CorneringSkill => corneringSkill;
        public VariableSO Drift => drift;
        public VariableSO Precision => precision;
        public VariableSO Smoothness => smoothness;

        internal void AssignVariablesForTests(VariableSO speedVariable)
        {
            speedCapability = speedVariable;
        }
    }
}
