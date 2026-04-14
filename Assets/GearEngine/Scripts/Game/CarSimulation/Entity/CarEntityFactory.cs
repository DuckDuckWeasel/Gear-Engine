using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Entity
{
    public sealed class CarEntityFactory
    {
        private readonly IInstanceIdGenerator idGenerator = new IncrementingInstanceIdGenerator();

        public CarEntity Create(CarDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var car = new CarEntity();
            car.Bind(definition);
            car.Initialize(idGenerator.Next(), definition);
            return car;
        }
    }
}
