using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Entity
{
    [System.Serializable]
    public class CarEntity : EntityInstance<CarDefinition>
    {
        public CarDefinition Definition { get; private set; }

        public GameObject CarPrefab => Definition?.CarPrefab;

        internal void Bind(CarDefinition definition)
        {
            Definition = definition;
        }
    }
}
