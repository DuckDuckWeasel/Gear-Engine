using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        public VariableSO Speed => speed;

        [FormerlySerializedAs("maxStraightSpeed")]
        [SerializeField] private VariableSO speed;

        internal void AssignVariablesForTests(VariableSO speedVariable)
        {
            speed = speedVariable;
        }
    }
}
