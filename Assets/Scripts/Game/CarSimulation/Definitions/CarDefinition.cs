using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation
{
    [CreateAssetMenu(menuName = "Game/Car/Car Definition", fileName = "CarDefinition")]
    public sealed class CarDefinition : EntityDefinition
    {
        [SerializeField] private GameObject carPrefab;
        public GameObject CarPrefab => carPrefab;
    }
}
