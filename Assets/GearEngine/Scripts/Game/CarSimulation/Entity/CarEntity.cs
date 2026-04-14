using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;
using UnityEngine;

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

        [SerializeField] private EntityInstance<CarDefinition> instance;

        public CarDefinition Definition => instance.Definition;

        private static readonly EntityInstanceCreator<CarDefinition> instanceCreator =
            new EntityInstanceCreator<CarDefinition>(new IncrementingInstanceIdGenerator());

        public static CarEntity Create(CarDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return new CarEntity(instanceCreator.Create(definition));
        }
    }
}
