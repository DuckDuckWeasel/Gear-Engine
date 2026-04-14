using System.IO;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Drivers;
using GearEngine.CarSimulation.Presentation;
using TrackViewComponent = GearEngine.CarSimulation.Track.Track;
using Scaffold.Entities;
using Scaffold.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.CarSimulation.Editor
{
    public static class CarSimulationSetupTool
    {
        private const string menuPath = "Game/Car Simulation/Setup Scene";
        private const string speedAssetPath = "Assets/Game/CarSimulation/Data/Speed.asset";
        private const string carDefinitionPath = "Assets/Game/CarSimulation/Data/CarDefinition.asset";
        private const string carPrefabPath = "Assets/Game/CarSimulation/Prefabs/Car.prefab";
        private const string circleTrackDefinitionPath = "Assets/Game/CarSimulation/Data/Tracks/CircleTrack.asset";
        private const string squareTrackDefinitionPath = "Assets/Game/CarSimulation/Data/Tracks/SquareTrack.asset";
        private const string scenePath = "Assets/Scenes/SplineTrack_TestScene.unity";
        private const float squareTrackHalfExtent = 30f;

        [MenuItem(menuPath)]
        public static void SetupScene()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder("Assets/Game/CarSimulation");
            EnsureFolder("Assets/Game/CarSimulation/Data");
            EnsureFolder("Assets/Game/CarSimulation/Data/Tracks");
            EnsureFolder("Assets/Game/CarSimulation/Prefabs");

            var speed = CreateOrLoadSpeedAsset();
            var carPrefab = CreateOrLoadCarPrefab(speed);
            var carDefinition = CreateOrLoadCarDefinition(speed, carPrefab);

            WireScene(carDefinition);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Car Simulation: setup complete.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static VariableSO CreateOrLoadSpeedAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VariableSO>(speedAssetPath);
            if (existing != null)
            {
                return existing;
            }

            var speed = ScriptableObject.CreateInstance<VariableSO>();
            AssetDatabase.CreateAsset(speed, speedAssetPath);
            var so = new SerializedObject(speed);
            so.FindProperty("valueType").enumValueIndex = (int)VariableValueType.Float;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(speed);
            return speed;
        }

        private static GameObject CreateOrLoadCarPrefab(VariableSO speed)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(carPrefabPath);
            if (existing != null)
            {
                EnsureCarPrefabDriverAndViewWired(speed);
                return AssetDatabase.LoadAssetAtPath<GameObject>(carPrefabPath);
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Car";
            ApplyCarVisual(go);
            var spline = ConfigureCarSpline(go);
            WireCarDriver(go, spline, speed);
            PrefabUtility.SaveAsPrefabAsset(go, carPrefabPath);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(carPrefabPath);
        }

        private static Shader FindUrpLitOrStandardShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }

        private static void ApplyCarVisual(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            var mat = new Material(FindUrpLitOrStandardShader()) { color = Color.red };
            renderer.sharedMaterial = mat;
        }

        private static SplineAnimate ConfigureCarSpline(GameObject go)
        {
            var spline = go.AddComponent<SplineAnimate>();
            spline.PlayOnAwake = false;
            spline.AnimationMethod = SplineAnimate.Method.Speed;
            spline.Easing = SplineAnimate.EasingMode.None;
            spline.Loop = SplineAnimate.LoopMode.Loop;
            return spline;
        }

        private static void EnsureCarPrefabDriverAndViewWired(VariableSO speed)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(carPrefabPath);
            try
            {
                RepairCarPrefabContentsIfSplinePresent(contents, speed);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void RepairCarPrefabContentsIfSplinePresent(GameObject contents, VariableSO speed)
        {
            var spline = contents.GetComponent<SplineAnimate>();
            if (spline == null)
            {
                Debug.LogError("Car Simulation: Car prefab must have SplineAnimate on root; cannot repair wiring.");
                return;
            }

            WireCarDriver(contents, spline, speed);
            PrefabUtility.SaveAsPrefabAsset(contents, carPrefabPath);
        }

        private static void WireCarDriver(GameObject go, SplineAnimate spline, VariableSO speed)
        {
            var driver = go.GetComponent<CarSplineDriver>() ?? go.AddComponent<CarSplineDriver>();
            var carView = go.GetComponent<CarView>() ?? go.AddComponent<CarView>();
            var driverSo = new SerializedObject(driver);
            driverSo.FindProperty("splineAnimate").objectReferenceValue = spline;
            driverSo.FindProperty("speedVariable").objectReferenceValue = speed;
            driverSo.ApplyModifiedPropertiesWithoutUndo();
            var carViewSo = new SerializedObject(carView);
            carViewSo.FindProperty("splineDriver").objectReferenceValue = driver;
            carViewSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(driver);
            EditorUtility.SetDirty(carView);
        }

        private static CarDefinition CreateOrLoadCarDefinition(VariableSO speed, GameObject carPrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<CarDefinition>(carDefinitionPath);
            if (existing != null)
            {
                return existing;
            }

            var def = ScriptableObject.CreateInstance<CarDefinition>();
            AssetDatabase.CreateAsset(def, carDefinitionPath);
            var so = new SerializedObject(def);
            WriteCarDefinitionEntries(so, carPrefab, speed);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            return def;
        }

        private static void WriteCarDefinitionEntries(SerializedObject defSo, GameObject carPrefab, VariableSO speed)
        {
            defSo.FindProperty("carPrefab").objectReferenceValue = carPrefab;
            SerializedProperty bagProp = defSo.FindProperty("bag");
            if (bagProp == null)
            {
                Debug.LogError("Car Simulation: CarDefinition has no serialized 'bag' field.");
                return;
            }

            SerializedProperty entries = bagProp.FindPropertyRelative("entries");
            entries.arraySize = 1;
            SerializedProperty e0 = entries.GetArrayElementAtIndex(0);
            e0.FindPropertyRelative("variable").objectReferenceValue = speed;
            e0.FindPropertyRelative("baseValue").managedReferenceValue = new FloatVariableValue { Value = 10f };
        }

        private static void WireScene(CarDefinition carDefinition)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!TryGetCircleRaceSpline(out var circleSpline))
            {
                return;
            }

            var circleTrackDef = CreateOrLoadCircleTrackDefinition(circleSpline);
            CreateOrLoadSquareTrackDefinition();

            CarSimulationNavigationAssetGenerator.Generate();
            EnsureAuthoringSplineOnlyOnCircleRaceHost();
            BindLifetimeScope(carDefinition, circleTrackDef);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static TrackDefinition CreateOrLoadCircleTrackDefinition(SplineContainer source)
        {
            var def = AssetDatabase.LoadAssetAtPath<TrackDefinition>(circleTrackDefinitionPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<TrackDefinition>();
                AssetDatabase.CreateAsset(def, circleTrackDefinitionPath);
            }

            CopySplineFromContainerToDefinition(source, def);
            var so = new SerializedObject(def);
            so.FindProperty("trackName").stringValue = "Circle";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            return def;
        }

        private static void CreateOrLoadSquareTrackDefinition()
        {
            var def = AssetDatabase.LoadAssetAtPath<TrackDefinition>(squareTrackDefinitionPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<TrackDefinition>();
                AssetDatabase.CreateAsset(def, squareTrackDefinitionPath);
            }

            WriteClosedSquareSpline(def.Spline, squareTrackHalfExtent);
            var so = new SerializedObject(def);
            so.FindProperty("trackName").stringValue = "Square";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
        }

        private static void CopySplineFromContainerToDefinition(SplineContainer container, TrackDefinition definition)
        {
            if (container == null || definition == null)
            {
                Debug.LogError("Car Simulation: Cannot copy spline — container or definition is null.");
                return;
            }

            var source = container.Spline;
            var dest = definition.Spline;
            dest.Knots = source.Knots;
            dest.Closed = source.Closed;
            EditorUtility.SetDirty(definition);
        }

        private static void WriteClosedSquareSpline(Spline spline, float halfExtent)
        {
            var h = halfExtent;
            spline.Knots = new[]
            {
                new BezierKnot(new Vector3(-h, 0, -h)),
                new BezierKnot(new Vector3(-h, 0, h)),
                new BezierKnot(new Vector3(h, 0, h)),
                new BezierKnot(new Vector3(h, 0, -h)),
            };
            spline.Closed = true;
            var range = new SplineRange(0, spline.Count);
            spline.SetTangentMode(range, TangentMode.Linear);
        }

        private static bool TryGetCircleRaceSpline(out SplineContainer container)
        {
            container = null;
            GameObject track = FindTrackGameObjectInScene();
            if (track == null)
            {
                Debug.LogError("Car Simulation: Track root not found in scene (expected GameObject named Track or CircleRaceTrack).");
                return false;
            }

            container = track.GetComponent<SplineContainer>();
            if (container == null)
            {
                Debug.LogError("Car Simulation: SplineContainer missing on track root.");
                return false;
            }

            return true;
        }

        private static GameObject FindTrackGameObjectInScene()
        {
            GameObject track = GameObject.Find("Track");
            if (track != null)
            {
                return track;
            }

            return GameObject.Find("CircleRaceTrack");
        }

        /// <summary>Sample: Keeps the scene spline for authoring <see cref="TrackDefinition"/>; runtime <see cref="GearEngine.CarSimulation.Track.Track"/> opens via navigation.</summary>
        private static void EnsureAuthoringSplineOnlyOnCircleRaceHost()
        {
            if (!TryFindCircleRaceParent(out GameObject parent))
            {
                return;
            }

            DestroyLegacyTrackChild(parent);
            EnsureSplineContainerOnParent(parent);
            RemoveLegacyTrackComponent(parent);
        }

        private static void DestroyLegacyTrackChild(GameObject parent)
        {
            Transform legacyChild = parent.transform.Find("Track");
            if (legacyChild != null)
            {
                Object.DestroyImmediate(legacyChild.gameObject);
            }
        }

        private static void EnsureSplineContainerOnParent(GameObject parent)
        {
            if (parent.GetComponent<SplineContainer>() == null)
            {
                parent.AddComponent<SplineContainer>();
            }
        }

        private static void RemoveLegacyTrackComponent(GameObject parent)
        {
            TrackViewComponent legacyTrack = parent.GetComponent<TrackViewComponent>();
            if (legacyTrack != null)
            {
                Object.DestroyImmediate(legacyTrack);
            }
        }

        private static bool TryFindCircleRaceParent(out GameObject parent)
        {
            parent = FindTrackGameObjectInScene();
            if (parent != null)
            {
                return true;
            }

            Debug.LogError("Car Simulation: Track root not found while wiring Car Simulation (expected Track or CircleRaceTrack).");
            return false;
        }

        private static CarTrackScope GetOrAddCarTrackScope()
        {
            const string scopeName = "CarTrack_LifetimeScope";
            var scopeGo = GameObject.Find(scopeName) ?? new GameObject(scopeName);
            return scopeGo.GetComponent<CarTrackScope>() ?? scopeGo.AddComponent<CarTrackScope>();
        }

        private static void BindLifetimeScope(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            CarTrackBootstrap bootstrap = WireBootstrapSerializedFields(carDefinition, trackDefinition);
            WireCarTrackScope(bootstrap);
        }

        private static CarTrackBootstrap WireBootstrapSerializedFields(CarDefinition carDefinition, TrackDefinition trackDefinition)
        {
            CarTrackBootstrap bootstrap = GetOrCreateCarTrackBootstrap();
            SerializedObject bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("trackDefinition").objectReferenceValue = trackDefinition;
            bootstrapSo.FindProperty("carDefinition").objectReferenceValue = carDefinition;
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            return bootstrap;
        }

        private static void WireCarTrackScope(CarTrackBootstrap bootstrap)
        {
            CarTrackScope scope = GetOrAddCarTrackScope();
            Transform holder = GetOrCreateNavigationViewHolder(scope.gameObject);
            SerializedObject scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("sceneBootstrap").objectReferenceValue = bootstrap;
            AssignNavigationSettingsToScope(scopeSo);
            scopeSo.FindProperty("navigationViewHolder").objectReferenceValue = holder;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(scope);
        }

        private static void AssignNavigationSettingsToScope(SerializedObject scopeSo)
        {
            var navSettings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(CarSimulationNavigationAssetGenerator.NavigationSettingsPath);
            if (navSettings != null)
            {
                scopeSo.FindProperty("navigationSettings").objectReferenceValue = navSettings;
                return;
            }

            Debug.LogError($"Car Simulation: Navigation Settings missing at {CarSimulationNavigationAssetGenerator.NavigationSettingsPath}.");
        }

        private static Transform GetOrCreateNavigationViewHolder(GameObject scopeGo)
        {
            const string holderName = "NavigationViewHolder";
            Transform existing = scopeGo.transform.Find(holderName);
            if (existing != null)
            {
                return existing;
            }

            var holder = new GameObject(holderName);
            holder.transform.SetParent(scopeGo.transform, false);
            return holder.transform;
        }

        private static CarTrackBootstrap GetOrCreateCarTrackBootstrap()
        {
            const string bootstrapName = "CarTrack_Bootstrap";
            GameObject bootstrapGo = GameObject.Find(bootstrapName);
            if (bootstrapGo == null)
            {
                bootstrapGo = new GameObject(bootstrapName);
            }

            return bootstrapGo.GetComponent<CarTrackBootstrap>() ?? bootstrapGo.AddComponent<CarTrackBootstrap>();
        }
    }
}
