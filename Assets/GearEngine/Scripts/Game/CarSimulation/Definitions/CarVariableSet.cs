using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        public VariableSO MaxStraightSpeed => maxStraightSpeed;

        [FormerlySerializedAs("speed")]
        [SerializeField] private VariableSO maxStraightSpeed;

        public VariableSO Acceleration => acceleration;

        [SerializeField] private VariableSO acceleration;

        public VariableSO Handling => handling;

        [SerializeField] private VariableSO handling;

        internal void AssignVariablesForTests(VariableSO maxStraight, VariableSO accel, VariableSO handle)
        {
            maxStraightSpeed = maxStraight;
            acceleration = accel;
            handling = handle;
        }
    }
}
