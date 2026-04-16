using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;
using UnityEngine;

namespace GearEngine.CarSimulation.Entity
{
    [Serializable]
    public class CarEntity: EntityInstance<CarDefinition>
    {
        public CarDefinition Definition => definition;
    }
}
