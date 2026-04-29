using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.SplineEvaluate.Definitions;
using GearEngine.SplineEvaluate.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.SplineEvaluate.Test2
{
    /// <summary>
    /// Lightweight bootstrap for the Test2 scene. Replaces
    /// <see cref="Bootstrap.SplineEvaluateBootstrap"/> by removing VContainer
    /// and SceneFoundation dependencies. Self-manages the
    /// <see cref="SplineEvaluateRunnerService"/> lifecycle via Update() ticking.
    /// </summary>
    public sealed class Test2Bootstrap : MonoBehaviour
    {
        [Header("Track")]
        [SerializeField] private SplineContainer splineContainer;

        [Header("Car")]
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private DriverPersonality personality = DriverPersonality.Default;

        [Header("Simulation Config")]
        [SerializeField] private SplineDriverConfig driverConfig;

        [Header("Optional")]
        [SerializeField] private LaneProfile laneProfileOverride;

        private GameObject spawnedCar;
        private SplineEvaluateDriver activeDriver;
        private SplineEvaluateRunnerService runnerService;

        public SplineEvaluateDriver ActiveDriver => activeDriver;

        private void Start()
        {
            try
            {
                Validate();
                InitializeRunner();
                SpawnAndStart();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Test2Bootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
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

        private void Update()
        {
            // Manually tick the runner service since there is no VContainer ITickable
            if (runnerService != null)
            {
                runnerService.Tick();
            }
        }

        private void Validate()
        {
            if (splineContainer == null || splineContainer.Spline == null || splineContainer.Spline.Count < 2)
            {
                throw new InvalidOperationException("[Test2Bootstrap] SplineContainer is missing or empty.");
            }

            if (driverConfig == null)
            {
                throw new InvalidOperationException("[Test2Bootstrap] Assign SplineDriverConfig.");
            }
        }

        private void InitializeRunner()
        {
            runnerService = new SplineEvaluateRunnerService(driverConfig);

            if (laneProfileOverride != null)
            {
                runnerService.SetDefaultLaneProfile(laneProfileOverride);
            }
        }

        private void SpawnAndStart()
        {
            // Spawn the car visual — use prefab if available, otherwise a debug cube
            if (carPrefab != null)
            {
                spawnedCar = Instantiate(carPrefab, transform);
            }
            else
            {
                Debug.LogWarning("[Test2Bootstrap] No car prefab assigned — using debug cube.");
                spawnedCar = CreateDebugCube();
            }

            // Create a minimal CarEntity using a dummy CarDefinition (no asset needed)
            var dummyDef = ScriptableObject.CreateInstance<CarDefinition>();
            var factory = new CarEntityFactory();
            CarEntity entity = factory.Create(dummyDef);

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
                Debug.LogError("[Test2Bootstrap] Failed to create driver.");
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
