using System;
using Scaffold.Entities;
using UnityEngine;

namespace Scaffold.CarSimulation
{
    [Serializable]
    public sealed class CarEntity
    {
        [SerializeField] private EntityInstance<CarDefinition> instance;

        private CarEntity(EntityInstance<CarDefinition> entityInstance)
        {
            instance = entityInstance ?? throw new ArgumentNullException(nameof(entityInstance));
        }

        public EntityInstance<CarDefinition> Instance => instance;

        public CarDefinition Definition => instance.Definition;

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
