using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Entity
{
    public sealed class CarEntityFactory
    {
        public CarEntity Create(CarDefinition carDefinition)
        {
            if (carDefinition == null)
            {
                throw new ArgumentNullException(nameof(carDefinition));
            }

            var car = new CarEntity(carDefinition, new LocalVariableStorage(null));
            return car;
        }
    }
}
