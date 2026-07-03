using GearEngine.CarSimulation;
using System;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using Scaffold.MVVM;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tracks
{
    [DisallowMultipleComponent]
    public sealed class TrackViewComponent : ViewComponent<ViewModel>
    {
        private const string pathChildName = "Path";

        public SplineContainer SplineContainer => splineContainer;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;

        [Header("Props")]
        [SerializeField] private GameObject startFinishLinePrefab;
        private GameObject startFinishLineInstance;

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

            if (viewModel is ITrackDefinitionSource source)
            {
                InitializeTrack(source.Track);
            }
            else
            {
                Debug.LogError("[Track] ViewModel must implement ITrackDefinitionSource.");
            }

            if (viewModel is TrackViewModel trackVm)
            {
                trackVm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        protected override void OnUnbind()
        {
            if (viewModel is TrackViewModel trackVm)
            {
                trackVm.PropertyChanged -= OnViewModelPropertyChanged;
                trackVm.TearDown();
            }

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

            CopySplineIntoContainer(data, splineContainer);
            RebuildVisualSplineExtrude(data);
            SpawnStartFinishLine();
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

        private void RebuildVisualSplineExtrude(TrackDefinition data)
        {
            if (splineExtrude == null)
            {
                return;
            }

            SyncVisualContainer(data);
            splineExtrude.Rebuild();
        }

        private void SyncVisualContainer(TrackDefinition data)
        {
            SplineContainer visualContainer = splineExtrude.Container;
            if (visualContainer == null)
            {
                splineExtrude.Container = splineContainer;
                return;
            }

            if (visualContainer != splineContainer)
            {
                CopySplineIntoContainer(data, visualContainer);
            }
        }

        private void CopySplineIntoContainer(TrackDefinition data, SplineContainer targetContainer)
        {
            Spline source = data.Spline;
            Spline target = targetContainer.Spline;
            target.Closed = source.Closed;
            target.Clear();
            foreach (var knot in source.Knots)
            {
                BezierKnot k = knot;
                k.Position = new Unity.Mathematics.float3(
                    k.Position.x * data.Scale + data.Offset.x, 
                    k.Position.y * data.Scale + data.Offset.y, 
                    k.Position.z * data.Scale + data.Offset.z);
                k.TangentIn = new Unity.Mathematics.float3(
                    k.TangentIn.x * data.Scale, 
                    k.TangentIn.y * data.Scale, 
                    k.TangentIn.z * data.Scale);
                k.TangentOut = new Unity.Mathematics.float3(
                    k.TangentOut.x * data.Scale, 
                    k.TangentOut.y * data.Scale, 
                    k.TangentOut.z * data.Scale);
                target.Add(k, TangentMode.AutoSmooth);
            }
        }

        private void SpawnStartFinishLine()
        {
            if (startFinishLinePrefab == null)
            {
                return;
            }
            
            if (startFinishLineInstance != null)
            {
                if (Application.isPlaying) Destroy(startFinishLineInstance);
                else DestroyImmediate(startFinishLineInstance);
            }

            if (splineContainer.Spline == null || splineContainer.Spline.Count == 0)
            {
                return;
            }

            Vector3 position = splineContainer.transform.TransformPoint(SplineUtility.EvaluatePosition(splineContainer.Spline, 0f));
            Vector3 forward = splineContainer.transform.TransformDirection(SplineUtility.EvaluateTangent(splineContainer.Spline, 0f));
            Vector3 up = splineContainer.transform.TransformDirection(SplineUtility.EvaluateUpVector(splineContainer.Spline, 0f));
            
            Quaternion rotation = Quaternion.LookRotation(forward, up);

            startFinishLineInstance = Instantiate(startFinishLinePrefab, position, rotation, this.transform);
        }
    }
}
