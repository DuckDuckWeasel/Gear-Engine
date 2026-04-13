using System;
using UnityEngine;
using UnityEngine.Splines;
using Scaffold.MVVM;

namespace Scaffold.CarSimulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    public sealed class Track : ViewComponent<TrackViewModel>
    {
        private const string pathChildName = "Path";

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;

        private CarView spawnedCarView;

        public SplineContainer SplineContainer => splineContainer;

        public new void Unbind()
        {
            base.Unbind();
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
            Bind<SimulationLifecycleState, SimulationLifecycleState>(() => viewModel.State, SyncSpawnedCarPlayback);
            if (viewModel.Car != null)
            {
                SpawnCarView(viewModel.Car);
            }

            SyncSpawnedCarPlayback(viewModel.State);
        }

        protected override void OnUnbind()
        {
            DestroyCarViewIfNeeded();
            viewModel?.TearDown();
            base.OnUnbind();
        }

        private void OnDestroy()
        {
            DestroyCarViewIfNeeded();
        }

        private void SyncSpawnedCarPlayback(SimulationLifecycleState state)
        {
            if (spawnedCarView == null || viewModel == null)
            {
                return;
            }

            spawnedCarView.OnRunningChanged(state);
        }

        private void SpawnCarView(CarEntity car)
        {
            DestroyCarViewIfNeeded();
            GameObject prefab = car.Definition.CarPrefab;
            if (prefab == null)
            {
                LogMissingCarPrefab();
                return;
            }

            InstantiateAndInitializeCarView(car, prefab);
        }

        private void LogMissingCarPrefab()
        {
            Debug.LogError("[Track] CarDefinition.CarPrefab is missing; cannot spawn CarView.");
        }

        private void InstantiateAndInitializeCarView(CarEntity car, GameObject prefab)
        {
            GameObject instance = Instantiate(prefab);
            PlaceCarUnderTrack(instance, prefab);
            if (!TryGetCarView(instance, out CarView view))
            {
                Destroy(instance);
                return;
            }

            spawnedCarView = view;
            FinalizeCarViewBinding(car, instance, view);
        }

        private void PlaceCarUnderTrack(GameObject instance, GameObject prefabAssetRoot)
        {
            Transform prefabTransform = prefabAssetRoot.transform;
            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(transform, false);
            instanceTransform.localPosition = prefabTransform.localPosition;
            instanceTransform.localRotation = prefabTransform.localRotation;
            instanceTransform.localScale = prefabTransform.localScale;
        }

        private void FinalizeCarViewBinding(CarEntity car, GameObject instance, CarView view)
        {
            if (viewModel == null)
            {
                CancelSpawnedCar(instance);
                return;
            }

            view.Initialize(car, splineContainer, viewModel);
            SyncSpawnedCarPlayback(viewModel.State);
        }

        private bool TryGetCarView(GameObject instance, out CarView view)
        {
            view = instance.GetComponent<CarView>();
            if (view != null)
            {
                return true;
            }

            Debug.LogError("[Track] Car prefab is missing CarView.");
            return false;
        }

        private void CancelSpawnedCar(GameObject instance)
        {
            Destroy(instance);
            spawnedCarView = null;
        }

        private void DestroyCarViewIfNeeded()
        {
            if (spawnedCarView == null)
            {
                return;
            }

            Destroy(spawnedCarView.gameObject);
            spawnedCarView = null;
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
    }
}
