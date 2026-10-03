using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.SplineSimulation;
using UnityEngine;
using UnityEngine.Splines;
using VContainer.Unity;

namespace GearEngine.CarSimulation.SplineSimulation
{
    /// <summary>
    /// Service that manages multiple <see cref="SplineEvaluateDriver"/> instances and
    /// ticks them each frame. Drop-in replacement for
    /// <see cref="GearEngine.CarSimulation.Simulation.SplineCarRunnerService"/> but
    /// without any physics or PrometeoCarController dependency.
    /// </summary>
    public sealed class SplineEvaluateRunnerService : ISimulationRunnerService, ITickable
    {
        private const float k_collisionHalfWidth = 1.1f;
        private const float k_collisionHalfLength = 2.2f;
        private const float k_collisionLateralSpeed = 3f;
        private const float k_collisionMaxFrameShift = 0.08f;

        /// <summary>Fired when a car completes a lap — mirrors <see cref="SplineCarRunnerService.OnLapCompleted"/>.</summary>
        public event Action<CarEntity> OnLapCompleted;

        private readonly SplineDriverConfig config;
        private readonly List<SplineEvaluateDriver> activeDrivers = new List<SplineEvaluateDriver>();
        private LaneProfile defaultLaneProfile;

        public SplineEvaluateRunnerService(SplineDriverConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>Sets the fallback lane profile used when no per-car profile is given.</summary>
        public void SetDefaultLaneProfile(LaneProfile laneProfile)
        {
            defaultLaneProfile = laneProfile;
        }

        /// <inheritdoc/>
        public void InitializeRun(ISimulationInitParams initParams)
        {
            if (initParams is not SplineInitParams spline)
            {
                throw new InvalidOperationException(
                    $"[SplineEvaluateRunnerService] Expected SplineInitParams but got {initParams?.GetType().Name}");
            }
            InitializeRun(spline.Track, spline.CarTransform, spline.Entity, spline.Personality, spline.LaneProfile);
        }

        /// <summary>
        /// Creates and registers a new driver for the given car on the given track spline.
        /// </summary>
        public SplineEvaluateDriver InitializeRun(
            SplineContainer trackContainer,
            Transform carTransform,
            CarEntity carEntity,
            DriverPersonality personality,
            LaneProfile laneProfile = null)
        {
            if (trackContainer == null || trackContainer.Spline == null || trackContainer.Spline.Count < 2)
            {
                Debug.LogError("[SplineEvaluateRunnerService] Invalid SplineContainer.");
                return null;
            }
            if (carTransform == null)
            {
                Debug.LogError("[SplineEvaluateRunnerService] carTransform is null.");
                return null;
            }
            if (carEntity == null)
            {
                Debug.LogError("[SplineEvaluateRunnerService] carEntity is null.");
                return null;
            }

            LaneProfile profile = laneProfile != null ? laneProfile : defaultLaneProfile;
            SplineEvaluateDriver driver = new SplineEvaluateDriver(config, profile);
            driver.Initialize(trackContainer, carTransform, carEntity, personality);
            driver.OnLapCompleted += HandleLapCompleted;
            activeDrivers.Add(driver);

            return driver;
        }

        /// <summary>Pauses or resumes a specific car.</summary>
        public void SetPaused(CarEntity entity, bool paused)
        {
            SplineEvaluateDriver driver = FindDriver(entity);
            if (driver != null)
            {
                driver.SetPaused(paused);
            }
        }

        public void ApplyJerk(CarEntity entity, float severity)
        {
            SplineEvaluateDriver driver = FindDriver(entity);
            if (driver != null)
            {
                driver.ApplyJerk(severity);
            }
        }

        /// <summary>
        /// Returns telemetry data compatible with the existing
        /// <see cref="CarTelemetryData"/> struct.
        /// </summary>
        public bool GetTelemetry(CarEntity entity, out CarTelemetryData data)
        {
            SplineEvaluateDriver driver = FindDriver(entity);
            if (driver != null)
            {
                SplineMotionState s = driver.State;
                data = new CarTelemetryData
                {
                    Speed = s.Speed * 3.6f, // m/s → km/h for display
                    MaxSpeed = config.maxSpeed * 3.6f,
                    Progress = s.T,
                    IsBraking = s.IsBraking,
                    IsDrifting = s.IsDrifting,
                    IsAccelerating = s.IsAccelerating,
                    CurrentAcceleration = s.IsAccelerating ? config.accelerationRate : 0f
                };
                return true;
            }

            data = default;
            return false;
        }

        /// <summary>Returns the driver for a given entity, or null.</summary>
        public SplineEvaluateDriver GetDriver(CarEntity entity)
        {
            return FindDriver(entity);
        }

        /// <summary>Removes a driver from the active list.</summary>
        public void RemoveDriver(CarEntity entity)
        {
            for (int i = activeDrivers.Count - 1; i >= 0; i--)
            {
                if (activeDrivers[i].CarEntity == entity)
                {
                    activeDrivers[i].OnLapCompleted -= HandleLapCompleted;
                    activeDrivers.RemoveAt(i);
                }
            }
        }

        public void TriggerCinematicFinish(CarEntity entity)
        {
            SplineEvaluateDriver driver = FindDriver(entity);
            if (driver != null)
            {
                driver.TriggerCinematicFinish();
            }
        }

        public void Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            for (int i = activeDrivers.Count - 1; i >= 0; i--)
            {
                SplineEvaluateDriver driver = activeDrivers[i];
                if (!driver.IsValid)
                {
                    activeDrivers.RemoveAt(i);
                    continue;
                }

                try
                {
                    driver.Tick(dt);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SplineEvaluateRunnerService] Tick failed for driver {i}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            ResolveCollisions(dt);
        }

        private void ResolveCollisions(float dt)
        {
            float contactDt = Mathf.Clamp(dt, 0f, 0.1f);
            if (contactDt <= 0f)
            {
                return;
            }

            for (int i = 0; i < activeDrivers.Count; i++)
            {
                SplineEvaluateDriver first = activeDrivers[i];
                if (!first.IsValid || first.IsPaused || first.IsInStartingGrid)
                {
                    continue;
                }

                for (int j = i + 1; j < activeDrivers.Count; j++)
                {
                    SplineEvaluateDriver second = activeDrivers[j];
                    if (!second.IsValid || second.IsPaused || second.IsInStartingGrid ||
                        first.TrackTransform != second.TrackTransform)
                    {
                        continue;
                    }

                    Transform trackTransform = first.TrackTransform;
                    Vector3 separation = trackTransform.InverseTransformVector(
                        second.CarTransform.position - first.CarTransform.position);
                    Vector3 localRight = trackTransform.InverseTransformDirection(first.CarTransform.right).normalized;
                    Vector3 localForward = trackTransform.InverseTransformDirection(first.CarTransform.forward).normalized;
                    Vector3 secondForward = trackTransform.InverseTransformDirection(second.CarTransform.forward).normalized;
                    if (Vector3.Dot(localForward, secondForward) < 0.7f)
                    {
                        continue;
                    }

                    float lateral = Vector3.Dot(separation, localRight);
                    float longitudinal = Vector3.Dot(separation, localForward);
                    float lateralOverlap = k_collisionHalfWidth * 2f - Mathf.Abs(lateral);
                    float longitudinalOverlap = k_collisionHalfLength * 2f - Mathf.Abs(longitudinal);
                    if (lateralOverlap <= 0f || longitudinalOverlap <= 0f || Mathf.Abs(separation.y) > 2f)
                    {
                        continue;
                    }

                    float direction = lateral < -0.01f ? -1f : 1f;
                    float displacement = Mathf.Min(lateralOverlap * 0.5f,
                        k_collisionLateralSpeed * contactDt, k_collisionMaxFrameShift);
                    float speedLoss = Mathf.Clamp01(longitudinalOverlap / (k_collisionHalfLength * 2f)) *
                        0.6f * contactDt;
                    first.ApplyCollisionImpact(-direction * displacement,
                        longitudinal >= 0f ? speedLoss : speedLoss * 0.2f);
                    second.ApplyCollisionImpact(direction * displacement,
                        longitudinal < 0f ? speedLoss : speedLoss * 0.2f);
                }
            }
        }

        private void HandleLapCompleted(CarEntity entity)
        {
            try
            {
                OnLapCompleted?.Invoke(entity);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SplineEvaluateRunnerService] OnLapCompleted handler error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private SplineEvaluateDriver FindDriver(CarEntity entity)
        {
            for (int i = 0; i < activeDrivers.Count; i++)
            {
                if (activeDrivers[i].CarEntity == entity)
                {
                    return activeDrivers[i];
                }
            }
            return null;
        }
    }
}
