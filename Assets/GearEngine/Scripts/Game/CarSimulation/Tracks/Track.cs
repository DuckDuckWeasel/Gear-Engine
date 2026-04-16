using System;
using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Presentation;
using Scaffold.MVVM;
using Unity.Mathematics;
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

        private CarView spawnedCarView;

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

        private void SpawnCarView(CarEntity car)
        {
            DestroyCarViewIfNeeded();
            GameObject prefab = car.CarPrefab;
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

            view.Initialize(car, splineContainer, viewModel.Simulation);
            SyncSpawnedCarPlayback(viewModel.State);
        }

        private void SyncSpawnedCarPlayback(SimulationLifecycleState state)
        {
            if (spawnedCarView == null || viewModel == null)
            {
                return;
            }

            spawnedCarView.OnRunningChanged(state);
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

    public sealed class SplineWaypointPath
    {
        internal SplineWaypointPath(IReadOnlyList<Vector3> pointsLocal, float totalLength, bool isClosed)
        {
            this.pointsLocal = pointsLocal;
            TotalLength = totalLength;
            this.isClosed = isClosed;
        }

        public int Count => pointsLocal.Count;

        public float TotalLength { get; }

        public bool IsClosed => isClosed;

        private readonly IReadOnlyList<Vector3> pointsLocal;

        private readonly bool isClosed;

        public float HorizontalDistanceToWaypoint(Vector3 worldPosition, int waypointIndex, Transform splineTransform)
        {
            Vector3 w = GetWorldPoint(waypointIndex, splineTransform);
            Vector3 a = worldPosition;
            Vector3 b = w;
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        public Vector3 GetWorldPoint(int index, Transform splineTransform)
        {
            return splineTransform.TransformPoint(GetLocalPoint(index));
        }

        public Vector3 GetLocalPoint(int index)
        {
            return pointsLocal[ClampIndex(index)];
        }

        public Vector3 EvaluateLookaheadWorld(int startWaypointIndex, Transform splineTransform, float lookaheadMetres)
        {
            if (pointsLocal.Count == 0)
            {
                return splineTransform.position;
            }

            float want = Mathf.Max(0f, lookaheadMetres);
            int idx = ClampIndex(startWaypointIndex);
            Vector3 worldA = splineTransform.TransformPoint(pointsLocal[idx]);
            if (want < 1e-4f)
            {
                return worldA;
            }

            return WalkLookaheadAlongWaypoints(splineTransform, want, idx, worldA);
        }

        private Vector3 WalkLookaheadAlongWaypoints(Transform splineTransform, float want, int idx, Vector3 worldA)
        {
            float remaining = want;
            int safety = Mathf.Max(pointsLocal.Count * 2, 8);
            while (remaining > 1e-4f && safety-- > 0)
            {
                LookaheadStepResult step = ComputeLookaheadStep(splineTransform, remaining, idx, worldA);
                if (step.Finished)
                {
                    return step.Position;
                }

                remaining = step.Remaining;
                idx = step.NextIndex;
                worldA = step.Position;
            }

            return worldA;
        }

        private LookaheadStepResult ComputeLookaheadStep(Transform splineTransform, float remaining, int idx, Vector3 worldA)
        {
            int nextIdx = NextWaypointIndex(idx);
            Vector3 worldB = splineTransform.TransformPoint(pointsLocal[nextIdx]);
            float seg = MeasureHorizontalDistance(worldA, worldB);
            if (seg < 1e-4f)
            {
                return new LookaheadStepResult(false, remaining, nextIdx, worldB);
            }

            if (remaining <= seg)
            {
                float t = remaining / seg;
                return new LookaheadStepResult(true, remaining, idx, Vector3.Lerp(worldA, worldB, t));
            }

            return new LookaheadStepResult(false, remaining - seg, nextIdx, worldB);
        }

        public int NextWaypointIndex(int index)
        {
            if (!isClosed)
            {
                return Mathf.Min(index + 1, pointsLocal.Count - 1);
            }

            int n = pointsLocal.Count;
            if (n <= 1)
            {
                return 0;
            }

            return (index + 1) % n;
        }

        private float MeasureHorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private int ClampIndex(int index)
        {
            if (pointsLocal.Count == 0)
            {
                return 0;
            }

            if (!isClosed)
            {
                return Mathf.Clamp(index, 0, pointsLocal.Count - 1);
            }

            int n = pointsLocal.Count;
            int m = index % n;
            return m < 0 ? m + n : m;
        }

        public static SplineWaypointPath Build(Spline spline, float spacingMetres)
        {
            return SplineWaypointPathBuild.Build(spline, spacingMetres);
        }

        private readonly struct LookaheadStepResult
        {
            internal LookaheadStepResult(bool finished, float remaining, int nextIndex, Vector3 position)
            {
                Finished = finished;
                Remaining = remaining;
                NextIndex = nextIndex;
                Position = position;
            }

            internal bool Finished { get; }

            internal float Remaining { get; }

            internal int NextIndex { get; }

            internal Vector3 Position { get; }
        }
    }

    internal static class SplineWaypointPathBuild
    {
        internal static SplineWaypointPath Build(Spline spline, float spacingMetres)
        {
            ValidateSpline(spline);
            float spacing = Mathf.Max(0.5f, spacingMetres);
            float length = spline.GetLength();
            if (length < 1e-4f)
            {
                throw new InvalidOperationException("Spline length is too small.");
            }

            bool closed = spline.Closed;
            List<Vector3> positions = SampleWaypointPositions(spline, spacing, length, closed);
            float perimeter = ComputePolylinePerimeter(positions, closed);
            float total = perimeter > 1e-4f ? perimeter : length;
            return new SplineWaypointPath(positions, total, closed);
        }

        private static void ValidateSpline(Spline spline)
        {
            if (spline == null)
            {
                throw new ArgumentNullException(nameof(spline));
            }

            if (spline.Count < 2)
            {
                throw new InvalidOperationException("Spline must contain at least two knots.");
            }
        }

        private static List<Vector3> SampleWaypointPositions(Spline spline, float spacing, float length, bool closed)
        {
            var positions = new List<Vector3>();
            AppendSamplesAlongLength(spline, spacing, length, positions);
            if (!closed)
            {
                AppendOpenSplineEnd(spline, positions);
            }
            else
            {
                EnsureClosedLoopSamples(spline, positions);
            }

            return positions;
        }

        private static void AppendSamplesAlongLength(Spline spline, float spacing, float length, List<Vector3> positions)
        {
            for (float d = 0f; d < length - 1e-4f; d += spacing)
            {
                float tNorm = d / length;
                SplineUtility.Evaluate(spline, tNorm, out float3 pos, out _, out _);
                positions.Add(pos);
            }
        }

        private static void AppendOpenSplineEnd(Spline spline, List<Vector3> positions)
        {
            SplineUtility.Evaluate(spline, 1f, out float3 endPos, out _, out _);
            if (positions.Count == 0 || math.distancesq(positions[positions.Count - 1], endPos) > 1e-6f)
            {
                positions.Add(endPos);
            }
        }

        private static void EnsureClosedLoopSamples(Spline spline, List<Vector3> positions)
        {
            if (positions.Count >= 2)
            {
                return;
            }

            SplineUtility.Evaluate(spline, 0f, out float3 p0, out _, out _);
            SplineUtility.Evaluate(spline, 0.5f, out float3 pm, out _, out _);
            positions.Clear();
            positions.Add(p0);
            positions.Add(pm);
        }

        private static float ComputePolylinePerimeter(IReadOnlyList<Vector3> positions, bool closed)
        {
            float perimeter = 0f;
            for (int i = 1; i < positions.Count; i++)
            {
                perimeter += Vector3.Distance(positions[i - 1], positions[i]);
            }

            if (closed && positions.Count >= 2)
            {
                perimeter += Vector3.Distance(positions[positions.Count - 1], positions[0]);
            }

            return perimeter;
        }
    }
}
