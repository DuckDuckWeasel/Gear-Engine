using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.PhysicsSimulation
{
    /// <summary>
    /// Initialization parameters specific to the physics simulation pipeline.
    /// Carries the <see cref="PrometeoCarController"/> and <see cref="RoguelikeCarStats"/>
    /// needed by <see cref="SplineCarRunnerService"/>.
    /// </summary>
    public sealed class PhysicsInitParams : ISimulationInitParams
    {
        public CarEntity Entity { get; set; }
        public SplineContainer Track { get; set; }
        public Transform CarTransform { get; set; }
        public PrometeoCarController Controller { get; set; }
        public RoguelikeCarStats Stats { get; set; }
    }
}
