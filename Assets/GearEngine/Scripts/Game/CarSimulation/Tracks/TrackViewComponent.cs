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
        public TrackThemeDefinition ActiveTheme => activeTheme;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private SplineExtrude splineExtrude;
        [SerializeField] private SplineExtrude roadEdgeExtrude;

        [Header("Theme")]
        [SerializeField] private MeshRenderer roadRenderer;
        [SerializeField] private MeshRenderer roadEdgeRenderer;
        [SerializeField] private MeshRenderer groundRenderer;
        [SerializeField] private SplinePropGenerator propGenerator;

        [Header("Props")]
        [SerializeField] private GameObject startFinishLinePrefab;
        private GameObject startFinishLineInstance;
        private GameObject environmentInstance;
        private GameObject effectsInstance;
        private TrackThemeDefinition activeTheme;
        private Material baseRoadMaterial;
        private Material baseRoadEdgeMaterial;
        private Material baseGroundMaterial;
        private bool baseGroundEnabled;
        private bool hasCachedRoadMaterial;
        private bool hasCachedRoadEdgeMaterial;
        private bool hasCachedGroundAppearance;

        public new void Unbind()
        {
            base.Unbind();
        }

        private void Awake()
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
            EnsureRoadEdgeReferences();
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

            ExecuteInitialize(data, data.VisualVariant);
        }

        public void InitializeTrack(TrackDefinition data, int visualVariant)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            ExecuteInitialize(data, visualVariant);
        }

        private void ExecuteInitialize(TrackDefinition data, int visualVariant)
        {
            EnsureSplineContainerReference();
            EnsureSplineExtrudeReference();
            EnsureRoadEdgeReferences();
            if (!HasSplineContainerOrLog() || !HasSplineDataOrLog(data))
            {
                return;
            }

            CopySplineIntoContainer(data, splineContainer);
            RebuildVisualSplineExtrude(data);
            ApplyTheme(data.Theme, visualVariant);
            SpawnStartFinishLine();
            GenerateProps();
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

        public void RefreshThemeEffects()
        {
            if (activeTheme == null)
            {
                return;
            }

            ClearEffects();
            SpawnEffects(activeTheme);
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

        private void EnsureRoadEdgeReferences()
        {
            if (roadEdgeExtrude != null && roadEdgeRenderer != null)
            {
                return;
            }

            Transform edge = transform.Find("Track/RoadEdge");
            if (edge == null)
            {
                return;
            }

            roadEdgeExtrude = edge.GetComponent<SplineExtrude>();
            roadEdgeRenderer = edge.GetComponent<MeshRenderer>();
        }

        private void ApplyTheme(TrackThemeDefinition theme, int visualVariant)
        {
            EnsureThemeReferences();
            CacheBaseAppearance();
            propGenerator?.ClearProps();
            ClearEnvironment();
            ClearEffects();
            RestoreBaseAppearance();
            activeTheme = theme;

            if (theme == null)
            {
                return;
            }

            ApplyThemeMaterials(theme);
            SpawnEnvironment(theme);
            ApplyVisualVariation(visualVariant);
            SpawnEffects(theme);
        }

        private void ApplyVisualVariation(int visualVariant)
        {
            Color tint = GetVariationTint(visualVariant);
            ApplyTint(groundRenderer, tint);
            if (environmentInstance == null)
            {
                return;
            }

            foreach (Renderer renderer in environmentInstance.GetComponentsInChildren<Renderer>(true))
            {
                ApplyTint(renderer, tint);
            }
        }

        private static Color GetVariationTint(int visualVariant)
        {
            switch (Mathf.Clamp(visualVariant, 0, 3))
            {
                case 1: return new Color(1.08f, 1.02f, 0.92f);
                case 2: return new Color(0.94f, 1.03f, 1.08f);
                case 3: return new Color(1.02f, 1.07f, 0.96f);
                default: return Color.white;
            }
        }

        private static void ApplyTint(Renderer renderer, Color tint)
        {
            if (renderer == null || renderer.sharedMaterial == null || !renderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                return;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", renderer.sharedMaterial.GetColor("_BaseColor") * tint);
            renderer.SetPropertyBlock(block);
        }

        private void EnsureThemeReferences()
        {
            EnsureRoadEdgeReferences();
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

            if (!hasCachedRoadEdgeMaterial && roadEdgeRenderer != null)
            {
                baseRoadEdgeMaterial = roadEdgeRenderer.sharedMaterial;
                hasCachedRoadEdgeMaterial = true;
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

            if (hasCachedRoadEdgeMaterial && roadEdgeRenderer != null)
            {
                roadEdgeRenderer.sharedMaterial = baseRoadEdgeMaterial;
                ApplyRoadEdgeColor(baseRoadMaterial);
            }

            if (hasCachedGroundAppearance && groundRenderer != null)
            {
                groundRenderer.sharedMaterial = baseGroundMaterial;
                groundRenderer.enabled = baseGroundEnabled;
                groundRenderer.SetPropertyBlock(null);
            }
        }

        private void SpawnEffects(TrackThemeDefinition theme)
        {
            if (theme.AmbientVfxPrefab == null && theme.SurfaceZones.Count == 0)
            {
                return;
            }

            effectsInstance = new GameObject("Track Effects");
            effectsInstance.transform.SetParent(GetOrCreateThemeRoot(), false);
            if (theme.AmbientVfxPrefab != null)
            {
                Instantiate(theme.AmbientVfxPrefab, effectsInstance.transform);
            }

            foreach (TrackThemeDefinition.SurfaceZone zone in theme.SurfaceZones)
            {
                SpawnSurfaceZone(zone);
            }
        }

        private void SpawnSurfaceZone(TrackThemeDefinition.SurfaceZone zone)
        {
            if (zone == null || zone.Material == null)
            {
                return;
            }

            float t = zone.NormalizedPosition;
            Vector3 position = splineContainer.transform.TransformPoint(SplineUtility.EvaluatePosition(splineContainer.Spline, t));
            Vector3 tangent = splineContainer.transform.TransformDirection(SplineUtility.EvaluateTangent(splineContainer.Spline, t));
            if (tangent.sqrMagnitude < 0.001f)
            {
                return;
            }

            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            patch.name = zone.Name;
            patch.transform.SetParent(effectsInstance.transform, false);
            float surfaceY = roadRenderer == null ? position.y + 0.08f : roadRenderer.bounds.max.y + 0.05f;
            patch.transform.position = new Vector3(position.x, surfaceY, position.z);
            patch.transform.rotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);
            patch.transform.localScale = new Vector3(
                zone.Width,
                0.02f,
                zone.NormalizedHalfLength * splineContainer.Spline.GetLength() * 2f);
            patch.GetComponent<MeshRenderer>().sharedMaterial = zone.Material;
            Collider collider = patch.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                DestroyObject(collider);
            }
        }

        private void ClearEffects()
        {
            if (effectsInstance == null)
            {
                return;
            }

            effectsInstance.SetActive(false);
            DestroyObject(effectsInstance);
            effectsInstance = null;
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private void ApplyThemeMaterials(TrackThemeDefinition theme)
        {
            if (roadRenderer != null && theme.RoadMaterial != null)
            {
                roadRenderer.sharedMaterial = theme.RoadMaterial;
            }

            if (roadEdgeRenderer != null && theme.RoadMaterial != null)
            {
                roadEdgeRenderer.sharedMaterial = theme.RoadMaterial;
                ApplyRoadEdgeColor(theme.RoadMaterial);
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

        private void ApplyRoadEdgeColor(Material roadMaterial)
        {
            if (roadEdgeRenderer == null || roadMaterial == null || !roadMaterial.HasProperty("_BaseColor"))
            {
                return;
            }

            Color roadColor = roadMaterial.GetColor("_BaseColor");
            Color.RGBToHSV(roadColor, out float hue, out float saturation, out float value);
            float edgeValue = Mathf.Max(0.5f, value + 0.3f);
            Color edgeColor = Color.HSVToRGB(hue, Mathf.Clamp01(saturation * 0.7f), Mathf.Clamp01(edgeValue));
            edgeColor.a = roadColor.a;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", edgeColor);
            roadEdgeRenderer.SetPropertyBlock(block);
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
            RebuildSplineExtrude(splineExtrude, data);
            RebuildSplineExtrude(roadEdgeExtrude, data);
        }

        private void RebuildSplineExtrude(SplineExtrude extrude, TrackDefinition data)
        {
            if (extrude == null)
            {
                return;
            }

            SplineContainer visualContainer = extrude.Container;
            if (visualContainer == null)
            {
                extrude.Container = splineContainer;
            }
            else if (visualContainer != splineContainer)
            {
                CopySplineIntoContainer(data, visualContainer);
            }

            extrude.Rebuild();
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
