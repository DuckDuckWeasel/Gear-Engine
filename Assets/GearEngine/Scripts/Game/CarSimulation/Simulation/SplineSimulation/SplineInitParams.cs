using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// Initialization parameters specific to the spline simulation pipeline.
    /// Carries the <see cref="DriverPersonality"/> and optional <see cref="LaneProfile"/>
    /// needed by <see cref="SplineEvaluateRunnerService"/>.
    /// </summary>
    public sealed class SplineInitParams : ISimulationInitParams
    {
        public CarEntity Entity { get; set; }
        public SplineContainer Track { get; set; }
        public Transform CarTransform { get; set; }
        public DriverPersonality Personality { get; set; }
        public LaneProfile LaneProfile { get; set; }
    }
}
