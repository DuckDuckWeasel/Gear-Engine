using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    /// <summary>
    /// Abstract base for all simulation config ScriptableObjects.
    /// The concrete type itself is the discriminator — no enum needed.
    /// The installer uses pattern matching (<c>is PhysicsSimulationConfig</c>)
    /// to register the correct runner.
    /// </summary>
    public abstract class SimulationConfigBase : ScriptableObject { }
}
