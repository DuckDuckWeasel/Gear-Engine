using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;
using VContainer.Unity;

namespace GearEngine.SplineEvaluate.Bootstrap
{
    /// <summary>
    /// Scene launcher for the spline-evaluate test scene. Creates a car entity,
    /// spawns the visual prefab, and wires the driver to the spline. Comparable
    /// to <see cref="GearEngine.CarSimulation.Bootstrap.CarTrackBootstrap"/> but
    /// physics-free.
    /// </summary>
    public sealed class SplineEvaluateBootstrap : MonoBehaviour, IInitializable
    {
        [Header("Track")]
        [SerializeField] private SplineContainer splineContainer;

        [Header("Car")]
        [SerializeField] private CarDefinition carDefinition;
        [SerializeField] private DriverPersonality personality = DriverPersonality.Default;

        [Header("Optional")]
        [SerializeField] private LaneProfile laneProfileOverride;

        [Inject] private SplineEvaluateRunnerService runnerService;

        private GameObject spawnedCar;
        private SplineEvaluateDriver activeDriver;

        public SplineEvaluateDriver ActiveDriver => activeDriver;

        public void Initialize()
        {
            try
            {
                Validate();
                SpawnAndStart();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SplineEvaluateBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Starts or resumes the active driver. Can be called from UI buttons.
        /// </summary>
        public void StartRace()
        {
            if (activeDriver != null)
            {
                activeDriver.SetPaused(false);
            }
        }

        /// <summary>
        /// Pauses the active driver. Can be called from UI buttons.
        /// </summary>
        public void PauseRace()
        {
            if (activeDriver != null)
            {
                activeDriver.SetPaused(true);
            }
        }

        /// <summary>
        /// Updates the personality at runtime (e.g. from stat sliders).
        /// </summary>
        public void UpdatePersonality(DriverPersonality newPersonality)
        {
            personality = newPersonality;
            if (activeDriver != null)
            {
                activeDriver.SetPersonality(newPersonality);
            }
        }

        private void Validate()
        {
            if (splineContainer == null || splineContainer.Spline == null || splineContainer.Spline.Count < 2)
            {
                throw new InvalidOperationException("[SplineEvaluateBootstrap] SplineContainer is missing or empty.");
            }

            if (carDefinition == null)
            {
                throw new InvalidOperationException("[SplineEvaluateBootstrap] CarDefinition is missing.");
            }

            if (carDefinition.CarPrefab == null)
            {
                throw new InvalidOperationException("[SplineEvaluateBootstrap] CarDefinition.CarPrefab is missing.");
            }
        }

        private void SpawnAndStart()
        {
            // Create runtime entity
            var factory = new CarEntityFactory();
            CarEntity entity = factory.Create(carDefinition);

            // Spawn visual prefab (mesh only — no Rigidbody, no PrometeoCarController)
            spawnedCar = Instantiate(carDefinition.CarPrefab, transform);

            // Initialize the driver
            LaneProfile profile = laneProfileOverride;
            activeDriver = runnerService.InitializeRun(
                splineContainer,
                spawnedCar.transform,
                entity,
                personality,
                profile);

            if (activeDriver == null)
            {
                Debug.LogError("[SplineEvaluateBootstrap] Failed to create driver.");
                return;
            }

            // Start paused — user or code calls StartRace()
            activeDriver.SetPaused(true);
        }

        private void OnDestroy()
        {
            if (activeDriver != null && activeDriver.CarEntity != null)
            {
                runnerService.RemoveDriver(activeDriver.CarEntity);
            }

            if (spawnedCar != null)
            {
                Destroy(spawnedCar);
            }
        }
    }
}
