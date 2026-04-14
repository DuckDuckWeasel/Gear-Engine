using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Entity
{
    [System.Serializable]
    public class CarEntity : EntityInstance<CarDefinition>
    {
        public GameObject CarPrefab => definition.CarPrefab;
    }
}
