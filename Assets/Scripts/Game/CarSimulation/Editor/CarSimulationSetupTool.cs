using System.IO;
using Game.CarSimulation;
using Scaffold.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace Game.CarSimulation.Editor
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

        private static AttributeSO CreateOrLoadSpeedAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AttributeSO>(speedAssetPath);
            if (existing != null)
            {
                return existing;
            }

            var speed = ScriptableObject.CreateInstance<AttributeSO>();
            AssetDatabase.CreateAsset(speed, speedAssetPath);
            var so = new SerializedObject(speed);
            so.FindProperty("valueType").enumValueIndex = (int)AttributeValueType.Float;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(speed);
            return speed;
        }

        private static GameObject CreateOrLoadCarPrefab(AttributeSO speed)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(carPrefabPath);
            if (existing != null)
            {
                return existing;
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

        private static void WireCarDriver(GameObject go, SplineAnimate spline, AttributeSO speed)
        {
            var driver = go.AddComponent<CarSplineDriver>();
            var driverSo = new SerializedObject(driver);
            driverSo.FindProperty("splineAnimate").objectReferenceValue = spline;
            driverSo.FindProperty("speedAttribute").objectReferenceValue = speed;
            driverSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CarDefinition CreateOrLoadCarDefinition(AttributeSO speed, GameObject carPrefab)
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

        private static void WriteCarDefinitionEntries(SerializedObject defSo, GameObject carPrefab, AttributeSO speed)
        {
            defSo.FindProperty("carPrefab").objectReferenceValue = carPrefab;
            var entries = defSo.FindProperty("entries");
            entries.arraySize = 1;
            var e0 = entries.GetArrayElementAtIndex(0);
            e0.FindPropertyRelative("attribute").objectReferenceValue = speed;
            e0.FindPropertyRelative("baseValue").managedReferenceValue = new FloatAttributeValue { Value = 10f };
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

            var trackBehaviour = GetOrAddTrackUnderCircleRaceTrack();
            BindLifetimeScope(carDefinition, circleTrackDef, trackBehaviour);

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
            var track = GameObject.Find("CircleRaceTrack");
            if (track == null)
            {
                Debug.LogError("Car Simulation: CircleRaceTrack not found in scene.");
                return false;
            }

            container = track.GetComponent<SplineContainer>();
            if (container == null)
            {
                Debug.LogError("Car Simulation: SplineContainer missing on CircleRaceTrack.");
                return false;
            }

            return true;
        }

        private static Track GetOrAddTrackUnderCircleRaceTrack()
        {
            if (!TryFindCircleRaceParent(out var parent))
            {
                return null;
            }

            var trackGo = GetOrCreateTrackChild(parent);
            return EnsureTrackAndSplineContainer(trackGo);
        }

        private static bool TryFindCircleRaceParent(out GameObject parent)
        {
            parent = GameObject.Find("CircleRaceTrack");
            if (parent != null)
            {
                return true;
            }

            Debug.LogError("Car Simulation: CircleRaceTrack not found while creating Track child.");
            return false;
        }

        private static GameObject GetOrCreateTrackChild(GameObject parent)
        {
            var trackTransform = parent.transform.Find("Track");
            if (trackTransform != null)
            {
                return trackTransform.gameObject;
            }

            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(parent.transform, false);
            trackGo.transform.localPosition = Vector3.zero;
            trackGo.transform.localRotation = Quaternion.identity;
            trackGo.transform.localScale = Vector3.one;
            return trackGo;
        }

        private static Track EnsureTrackAndSplineContainer(GameObject trackGo)
        {
            var container = trackGo.GetComponent<SplineContainer>() ?? trackGo.AddComponent<SplineContainer>();
            var track = trackGo.GetComponent<Track>() ?? trackGo.AddComponent<Track>();
            var trackSo = new SerializedObject(track);
            trackSo.FindProperty("splineContainer").objectReferenceValue = container;
            trackSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(track);
            return track;
        }

        private static CarTrackScope GetOrAddCarTrackScope()
        {
            const string scopeName = "CarTrack_LifetimeScope";
            var scopeGo = GameObject.Find(scopeName) ?? new GameObject(scopeName);
            return scopeGo.GetComponent<CarTrackScope>() ?? scopeGo.AddComponent<CarTrackScope>();
        }

        private static void BindLifetimeScope(CarDefinition carDefinition, TrackDefinition trackDefinition, Track track)
        {
            if (track == null)
            {
                Debug.LogError("Car Simulation: Track component is null; cannot bind CarTrackScope.");
                return;
            }

            var scope = GetOrAddCarTrackScope();
            var scopeSo = new SerializedObject(scope);
            scopeSo.FindProperty("carDefinition").objectReferenceValue = carDefinition;
            scopeSo.FindProperty("trackDefinition").objectReferenceValue = trackDefinition;
            scopeSo.FindProperty("track").objectReferenceValue = track;
            scopeSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(scope);
        }
    }
}
