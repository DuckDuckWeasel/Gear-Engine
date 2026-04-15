using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    public interface IRaceRandom
    {
        float NextFloat();
    }

    internal sealed class UnityRaceRandom : IRaceRandom
    {
        public float NextFloat()
        {
            return Random.value;
        }
    }
}
