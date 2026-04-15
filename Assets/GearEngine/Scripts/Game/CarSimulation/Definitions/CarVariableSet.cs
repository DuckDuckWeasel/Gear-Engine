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

        public VariableSO MaxCurveSpeed => maxCurveSpeed;

        [SerializeField] private VariableSO maxCurveSpeed;

        public VariableSO Acceleration => acceleration;

        [SerializeField] private VariableSO acceleration;

        public VariableSO Brake => brake;

        [SerializeField] private VariableSO brake;

        public VariableSO Handling => handling;

        [SerializeField] private VariableSO handling;

        internal void AssignVariablesForTests(VariableSO maxStraight, VariableSO maxCurve, VariableSO accel, VariableSO brakeVar, VariableSO handle)
        {
            maxStraightSpeed = maxStraight;
            maxCurveSpeed = maxCurve;
            acceleration = accel;
            brake = brakeVar;
            handling = handle;
        }
    }
}
