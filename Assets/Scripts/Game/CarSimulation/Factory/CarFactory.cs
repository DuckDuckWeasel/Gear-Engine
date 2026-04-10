using Scaffold.Entities;
using UnityEngine;

namespace Game.CarSimulation
{
    public class CarFactory
    {
        public CarEntity Create(CarDefinition definition)
        {
            var carGO = Object.Instantiate(definition.CarPrefab);
            return EntityInstanceFactory.CreateOnGameObject<CarEntity, CarDefinition>(carGO, definition);
        }
    }
}
