using System;
using Scaffold.Entities;
using UnityEngine;
using GearEngine.CarSimulation.Definitions;

namespace GearEngine.CarSimulation.Entity
{
    [Serializable]
    public sealed class CarEntity
    {
        private CarEntity(EntityInstance<CarDefinition> entityInstance)
        {
            instance = entityInstance ?? throw new ArgumentNullException(nameof(entityInstance));
        }

        public EntityInstance<CarDefinition> Instance => instance;

        public CarDefinition Definition => instance.Definition;

        [SerializeField] private EntityInstance<CarDefinition> instance;

        public static CarEntity Create(CarDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return new CarEntity(EntityInstanceFactory.CreateInstance(definition));
        }
    }
}
