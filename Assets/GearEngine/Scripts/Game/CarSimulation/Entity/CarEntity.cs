using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Entity
{
    [Serializable]
    public sealed class CarEntity : EntityInstance<CarDefinition>
    {
        public CarDefinition Definition => definition;
    }
}
