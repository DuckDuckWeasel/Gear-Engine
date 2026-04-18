using System;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Presentation;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tracks
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    public sealed class Track : ViewComponent<TrackViewModel>
    {
        private const string pathChildName = "Path";

        public SplineContainer SplineContainer => splineContainer;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;

        private GameObject spawnedDynamicCar;

        public void ReleaseViewBinding()
        {
            Unbind();
        }

        private void Awake()
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
        }

        protected override void OnBind()
        {
            if (viewModel == null)
            {
                return;
            }

            InitializeTrack(viewModel.Track);
            TryBindRaceSessionToScene();
            if (viewModel.SpawnCarWhenSessionStartsRunning)
            {
                Bind<SimulationLifecycleState, SimulationLifecycleState>(() => viewModel.State, OnTrackStateChangedForDeferredCarSpawn);
            }
        }

        protected override void OnUnbind()
        {
            DestroySpawnedDynamicCar();
            viewModel?.TearDown();
            base.OnUnbind();
        }

        private void InitializeTrack(TrackDefinition data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ExecuteInitialize(data);
        }

        private void ExecuteInitialize(TrackDefinition data)
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
            if (!HasSplineContainerOrLog() || !HasSplineDataOrLog(data))
            {
                return;
            }

            CopySplineIntoContainer(data.Spline, splineContainer);
            RebuildVisualSplineExtrude(data.Spline);
        }

        private bool HasSplineContainerOrLog()
        {
            if (splineContainer != null)
            {
                return true;
            }

            LogSplineContainerMissing();
            return false;
        }

        private bool HasSplineDataOrLog(TrackDefinition data)
        {
            if (data.Spline.Count > 0)
            {
                return true;
            }

            LogEmptyTrackDefinition(data.name);
            return false;
        }

        private void EnsureSplineContainerReference()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();
            }
        }

        private void EnsureSplineExtrudeReference()
        {
            if (splineExtrude != null)
            {
                return;
            }

            Transform path = transform.Find(pathChildName);
            if (path != null)
            {
                splineExtrude = path.GetComponent<SplineExtrude>();
            }

            if (splineExtrude == null)
            {
                splineExtrude = GetComponent<SplineExtrude>();
            }
        }

        private void LogSplineContainerMissing()
        {
            Debug.LogError("[Track] SplineContainer is missing; cannot Initialize.");
        }

        private void LogEmptyTrackDefinition(string definitionName)
        {
            Debug.LogError($"[Track] TrackDefinition '{definitionName}' has no spline knots.");
        }

        private void RebuildVisualSplineExtrude(Spline source)
        {
            if (splineExtrude == null)
            {
                return;
            }

            SyncVisualContainer(source);
            splineExtrude.Rebuild();
        }

        private void SyncVisualContainer(Spline source)
        {
            SplineContainer visualContainer = splineExtrude.Container;
            if (visualContainer == null)
            {
                splineExtrude.Container = splineContainer;
                return;
            }

            if (visualContainer != splineContainer)
            {
                CopySplineIntoContainer(source, visualContainer);
            }
        }

        private void CopySplineIntoContainer(Spline source, SplineContainer targetContainer)
        {
            Spline target = targetContainer.Spline;
            target.Knots = source.Knots;
            target.Closed = source.Closed;
        }

        private void TryBindRaceSessionToScene()
        {
            LapRaceSession session = viewModel.Session;
            if (session == null)
            {
                return;
            }

            session.BindSpline(SplineContainer);
            TryInitializeExistingOrSpawnCar(session);
        }

        private void TryInitializeExistingOrSpawnCar(LapRaceSession session)
        {
            CarView carView = GetComponentInChildren<CarView>(true);
            if (carView != null)
            {
                carView.Initialize(session.Car, SplineContainer, session);
                return;
            }

            if (viewModel.SpawnCarOnBindIfNoChild)
            {
                TrySpawnDynamicCarForSession(session);
            }
        }

        private void OnTrackStateChangedForDeferredCarSpawn(SimulationLifecycleState state)
        {
            if (state != SimulationLifecycleState.Running)
            {
                return;
            }

            if (GetComponentInChildren<CarView>(true) != null)
            {
                return;
            }

            LapRaceSession session = viewModel.Session;
            if (session == null)
            {
                return;
            }

            TrySpawnDynamicCarForSession(session);
        }

        private void TrySpawnDynamicCarForSession(LapRaceSession session)
        {
            if (!TryResolveCarPrefab(session, out GameObject prefab))
            {
                return;
            }

            GameObject instance = Instantiate(prefab, transform);
            if (!instance.TryGetComponent(out CarView newCarView))
            {
                Debug.LogError("[Track] Spawned CarPrefab is missing a CarView component.");
                DestroySpawnedGameObject(instance);
                return;
            }

            spawnedDynamicCar = instance;
            newCarView.Initialize(session.Car, SplineContainer, session);
        }

        private bool TryResolveCarPrefab(LapRaceSession session, out GameObject prefab)
        {
            prefab = null;
            CarEntity car = session.Car;
            if (car?.Definition == null)
            {
                Debug.LogError("[Track] Cannot spawn car: session.Car or CarDefinition is missing.");
                return false;
            }

            prefab = car.Definition.CarPrefab;
            if (prefab == null)
            {
                Debug.LogError("[Track] Cannot spawn car: CarDefinition.CarPrefab is not assigned.");
                return false;
            }

            return true;
        }

        private void DestroySpawnedDynamicCar()
        {
            DestroySpawnedGameObject(spawnedDynamicCar);
            spawnedDynamicCar = null;
        }

        private void DestroySpawnedGameObject(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(instance);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
