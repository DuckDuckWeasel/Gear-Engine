using System;
using GearEngine.CarSimulation.Entity;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Simulation
{
    /// <summary>
    /// Common contract for both physics and spline simulation runners.
    /// The bootstrap calls <see cref="InitializeRun"/> with pipeline-specific params;
    /// <see cref="RaceManagerService"/> only sees this interface.
    /// </summary>
    public interface ISimulationRunnerService
    {
        /// <summary>Fired when a car's progress wraps past 1.0 (one lap completed).</summary>
        event Action<CarEntity> OnLapCompleted;

        /// <summary>
        /// Initializes a car on a track. The concrete <see cref="ISimulationInitParams"/>
        /// carries pipeline-specific data (Rigidbody vs pure-math).
        /// </summary>
        void InitializeRun(ISimulationInitParams initParams);

        /// <summary>Pauses or resumes a specific car.</summary>
        void SetPaused(CarEntity entity, bool paused);

        /// <summary>Returns telemetry data for a given car entity.</summary>
        bool GetTelemetry(CarEntity entity, out CarTelemetryData data);

        /// <summary>Removes a driver from the active simulation.</summary>
        void RemoveDriver(CarEntity entity);

        /// <summary>Advances the simulation by one frame.</summary>
        void Tick();
    }

    /// <summary>
    /// Marker interface for pipeline-specific initialization parameters.
    /// Each pipeline defines its own concrete class.
    /// </summary>
    public interface ISimulationInitParams
    {
        /// <summary>The identity entity for this car.</summary>
        CarEntity Entity { get; }

        /// <summary>The spline track container.</summary>
        SplineContainer Track { get; }

        /// <summary>The car's visual transform to be driven by the simulation.</summary>
        Transform CarTransform { get; }
    }
}
