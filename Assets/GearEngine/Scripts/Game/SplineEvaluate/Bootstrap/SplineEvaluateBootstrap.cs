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
                Debug.LogWarning("[SplineEvaluateBootstrap] No CarDefinition assigned — using debug placeholder.");
            }
        }

        private void SpawnAndStart()
        {
            CarEntity entity;
            if (carDefinition != null)
            {
                var factory = new CarEntityFactory();
                entity = factory.Create(carDefinition);

                if (carDefinition.CarPrefab != null)
                {
                    spawnedCar = Instantiate(carDefinition.CarPrefab, transform);
                }
                else
                {
                    Debug.LogWarning("[SplineEvaluateBootstrap] CarDefinition has no CarPrefab — using debug cube.");
                    spawnedCar = CreateDebugCube();
                }
            }
            else
            {
                // Create a minimal entity + placeholder visual
                var dummyDef = ScriptableObject.CreateInstance<CarDefinition>();
                var factory = new CarEntityFactory();
                entity = factory.Create(dummyDef);
                spawnedCar = CreateDebugCube();
            }

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

        private GameObject CreateDebugCube()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "DebugCar";
            cube.transform.SetParent(transform, false);
            cube.transform.localScale = new Vector3(2f, 1f, 4f);

            // Remove collider to avoid physics interference
            var collider = cube.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            // Give it a visible color
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(1f, 0.3f, 0.1f);
                renderer.sharedMaterial = mat;
            }

            return cube;
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
