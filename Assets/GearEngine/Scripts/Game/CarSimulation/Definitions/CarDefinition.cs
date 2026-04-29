using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Car Definition", fileName = "CarDefinition")]
    public sealed class CarDefinition : EntityDefinitionAsset
    {
        public GameObject CarPrefab => carPrefab;

        [SerializeField] private GameObject carPrefab;
    }
}
