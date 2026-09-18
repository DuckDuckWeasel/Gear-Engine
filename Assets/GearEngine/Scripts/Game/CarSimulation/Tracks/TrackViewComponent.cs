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
        public SplineContainer SplineContainer => splineContainer;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;

        [Header("Theme")]
        [SerializeField] private MeshRenderer roadRenderer;
        [SerializeField] private MeshRenderer groundRenderer;
        [SerializeField] private SplinePropGenerator propGenerator;

        [Header("Props")]
        [SerializeField] private GameObject startFinishLinePrefab;
        private GameObject startFinishLineInstance;
        private GameObject environmentInstance;
        private TrackThemeDefinition activeTheme;
        private Material baseRoadMaterial;
        private Material baseGroundMaterial;
        private bool baseGroundEnabled;
        private bool hasCachedRoadMaterial;
        private bool hasCachedGroundAppearance;

        public new void Unbind()
        {
            base.Unbind();
        }

        private void Awake()
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
            EnsureThemeReferences();
            CacheBaseAppearance();
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

        public void InitializeTrack(TrackDefinition data)
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
            ApplyTheme(data.Theme);
            SpawnStartFinishLine();
        }

        public void GenerateProps()
        {
            EnsureThemeReferences();
            if (propGenerator == null)
            {
                return;
            }

            propGenerator.track = splineContainer;
            if (activeTheme == null || activeTheme.UsesDefaultProps)
            {
                propGenerator.Generate();
                return;
            }

            propGenerator.Generate(activeTheme.PropRules);
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

            Transform path = transform.Find("Path");
            if (path != null)
            {
                splineExtrude = path.GetComponent<SplineExtrude>();
            }

            if (splineExtrude == null)
            {
                splineExtrude = GetComponent<SplineExtrude>();
            }
        }

        private void ApplyTheme(TrackThemeDefinition theme)
        {
            EnsureThemeReferences();
            CacheBaseAppearance();
            propGenerator?.ClearProps();
            ClearEnvironment();
            RestoreBaseAppearance();
            activeTheme = theme;

            if (theme == null)
            {
                return;
            }

            ApplyThemeMaterials(theme);
            SpawnEnvironment(theme);
        }

        private void EnsureThemeReferences()
        {
            if (roadRenderer == null)
            {
                Transform path = transform.Find("Track/Path");
                roadRenderer = path == null ? null : path.GetComponent<MeshRenderer>();
            }

            if (groundRenderer == null)
            {
                Transform floor = transform.Find("Floor");
                groundRenderer = floor == null ? null : floor.GetComponent<MeshRenderer>();
            }

            if (propGenerator == null)
            {
                propGenerator = GetComponentInChildren<SplinePropGenerator>(true);
            }
        }

        private void CacheBaseAppearance()
        {
            if (!hasCachedRoadMaterial && roadRenderer != null)
            {
                baseRoadMaterial = roadRenderer.sharedMaterial;
                hasCachedRoadMaterial = true;
            }

            if (!hasCachedGroundAppearance && groundRenderer != null)
            {
                baseGroundMaterial = groundRenderer.sharedMaterial;
                baseGroundEnabled = groundRenderer.enabled;
                hasCachedGroundAppearance = true;
            }
        }

        private void RestoreBaseAppearance()
        {
            if (hasCachedRoadMaterial && roadRenderer != null)
            {
                roadRenderer.sharedMaterial = baseRoadMaterial;
            }

            if (hasCachedGroundAppearance && groundRenderer != null)
            {
                groundRenderer.sharedMaterial = baseGroundMaterial;
                groundRenderer.enabled = baseGroundEnabled;
            }
        }

        private void ApplyThemeMaterials(TrackThemeDefinition theme)
        {
            if (roadRenderer != null && theme.RoadMaterial != null)
            {
                roadRenderer.sharedMaterial = theme.RoadMaterial;
            }

            if (groundRenderer == null)
            {
                return;
            }

            if (theme.GroundMaterial != null)
            {
                groundRenderer.sharedMaterial = theme.GroundMaterial;
            }

            groundRenderer.enabled = !theme.HidesBaseGround;
        }

        private void SpawnEnvironment(TrackThemeDefinition theme)
        {
            if (theme.EnvironmentPrefab == null)
            {
                return;
            }

            Transform themeRoot = GetOrCreateThemeRoot();
            environmentInstance = Instantiate(theme.EnvironmentPrefab, themeRoot);
            Transform environmentTransform = environmentInstance.transform;
            environmentTransform.localPosition = theme.EnvironmentLocalPosition;
            environmentTransform.localRotation = Quaternion.Euler(theme.EnvironmentLocalEulerAngles);
            environmentTransform.localScale = theme.EnvironmentLocalScale;
        }

        private Transform GetOrCreateThemeRoot()
        {
            Transform themeRoot = transform.Find("Track Theme");
            if (themeRoot != null)
            {
                return themeRoot;
            }

            GameObject root = new GameObject("Track Theme");
            themeRoot = root.transform;
            themeRoot.SetParent(transform, false);
            return themeRoot;
        }

        private void ClearEnvironment()
        {
            if (environmentInstance == null)
            {
                return;
            }

            environmentInstance.SetActive(false);
            if (Application.isPlaying)
            {
                Destroy(environmentInstance);
            }
            else
            {
                DestroyImmediate(environmentInstance);
            }

            environmentInstance = null;
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
            foreach (BezierKnot knot in source.Knots)
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
                if (Application.isPlaying)
                {
                    Destroy(startFinishLineInstance);
                }
                else
                {
                    DestroyImmediate(startFinishLineInstance);
                }
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
