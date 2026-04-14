using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Variable Set", fileName = "CarVariableSet")]
    public sealed class CarVariableSet : ScriptableObject
    {
        public VariableSO Speed => speed;

        [SerializeField] private VariableSO speed;
    }
}
