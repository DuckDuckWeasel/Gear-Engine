using System.Collections.Generic;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackViewComponentDynamicCarTests
    {
        [Test, Category("Biome")]
        public void ThemeSurfaces_OnlyGlacialIcePushesTheCarSideways()
        {
            string[] names = { "Desert", "Forest", "Mountain", "Glacial" };
            foreach (string name in names)
            {
                TrackThemeDefinition theme = AssetDatabase.LoadAssetAtPath<TrackThemeDefinition>(
                    $"Assets/GearEngine/Data/Track/Themes/{name}TrackTheme.asset");
                Assert.That(theme, Is.Not.Null, $"{name} theme is missing.");
                Assert.That(theme.SurfaceZones, Has.Count.EqualTo(1));
                if (name == "Glacial")
                {
                    Assert.That(Mathf.Abs(theme.SurfaceZones[0].LateralPush), Is.GreaterThan(0f));
                }
                else
                {
                    Assert.That(theme.SurfaceZones[0].LateralPush, Is.EqualTo(0f), $"{name} should not slide the car.");
                }
            }
        }

        [Test, Category("Biome")]
        public void ThemeGroundTextures_MirrorAtTileEdges()
        {
            string[] texturePaths =
            {
                "Assets/GearEngine/Art/ModelsTextures/Background/Desert/Textures/T_Desert_BaseColor.png",
                "Assets/GearEngine/Art/ModelsTextures/Background/Forest/Texture/T_Forest_BaseColor.png",
                "Assets/GearEngine/Art/ModelsTextures/Background/Rock/Textures/T_Rock_BaseColor.png",
                "Assets/GearEngine/Art/ModelsTextures/Background/Glacial/Textures/T_Glacial_BaseColor.png",
            };

            foreach (string path in texturePaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, $"Missing ground texture importer: {path}");
                Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Mirror),
                    $"The ground texture must not expose mismatched source edges: {path}");
            }
        }

        [TestCase(typeof(SplinePropGenerator.All))]
        [TestCase(typeof(SplinePropGenerator.Curves))]
        [TestCase(typeof(SplinePropGenerator.Straights))]
        [TestCase(typeof(SplinePropGenerator.Inside))]
        [TestCase(typeof(SplinePropGenerator.Outside))]
        public void PropRuleType_IsPreservedForPlayerSerialization(System.Type ruleType)
        {
            Assert.That(
                System.Attribute.IsDefined(ruleType, typeof(PreserveAttribute)),
                Is.True,
                $"{ruleType.Name} must survive IL2CPP stripping for SerializeReference theme rules.");
        }

        [Test]
        public void Bind_WithTrackViewModel_InitializesSplineFromTrackDefinition()
        {
            GameObject trackGo = new GameObject("TrackHarnessBind");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    SeedOpenSpline(trackDef);

                    PhysicsSimulationConfig carRunnerConfig = ScriptableObject.CreateInstance<PhysicsSimulationConfig>();
                    SplineCarRunnerService carRunner = new SplineCarRunnerService(carRunnerConfig);
                    RaceManagerService raceManager = new RaceManagerService(carRunner);
                    TrackSimulationFactory factory = new TrackSimulationFactory();

                    RaceState session = factory.Create(carDef, trackDef, null);
                    raceManager.RegisterRace(session);
                    TrackViewModel trackVm = new TrackViewModel(session, raceManager, carRunner, factory);
                    track.Bind(trackVm);

                    Assert.That(trackGo.GetComponent<SplineContainer>().Spline.Count, Is.GreaterThan(0));
                    track.Unbind();

                    Object.DestroyImmediate(carRunnerConfig);
                }
                finally
                {
                    Object.DestroyImmediate(carDef);
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
            }
        }

        [Test]
        public void InitializeTrack_WithThemeRules_GeneratesPropsWithoutViewSpecificCallback()
        {
            GameObject trackGo = new GameObject("TrackHarnessProps");
            GameObject propPrefab = new GameObject("PropPrefab");
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            TrackThemeDefinition theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();
            try
            {
                SplineContainer splineContainer = trackGo.AddComponent<SplineContainer>();
                SplinePropGenerator propGenerator = trackGo.AddComponent<SplinePropGenerator>();
                propGenerator.track = splineContainer;
                TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
                SeedOpenSpline(trackDef);

                SerializedObject serializedTheme = new SerializedObject(theme);
                serializedTheme.FindProperty("useDefaultProps").boolValue = false;
                SerializedProperty rules = serializedTheme.FindProperty("propRules");
                rules.arraySize = 1;
                rules.GetArrayElementAtIndex(0).managedReferenceValue = new SplinePropGenerator.All
                {
                    prefabs = new List<GameObject> { propPrefab },
                    chancePerSegment = 1f,
                    minAmount = 1,
                    maxAmount = 1,
                    side = SplinePropGenerator.PlacementSide.Left,
                    leftDistance = 11f,
                    minimumSplineClearance = 8.5f,
                };
                serializedTheme.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject serializedTrack = new SerializedObject(trackDef);
                serializedTrack.FindProperty("theme").objectReferenceValue = theme;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();

                track.InitializeTrack(trackDef);

                Assert.That(propGenerator.generatedProps, Has.Count.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
                Object.DestroyImmediate(propPrefab);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(theme);
            }
        }

        [Test, Category("Biome")]
        public void InitializeTrack_SurfacePatchMatchesGameplayZoneAndHasNoCollider()
        {
            GameObject trackGo = new GameObject("TrackHarnessSurface");
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            TrackThemeDefinition theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();
            try
            {
                SplineContainer splineContainer = trackGo.AddComponent<SplineContainer>();
                TrackViewComponent track = trackGo.AddComponent<TrackViewComponent>();
                SeedOpenSpline(trackDef);

                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/GearEngine/Art/Materials/TrackThemes/M_ThemeDesertSurface.mat");
                Assert.That(material, Is.Not.Null);
                SerializedObject serializedTheme = new SerializedObject(theme);
                SerializedProperty zones = serializedTheme.FindProperty("surfaceZones");
                zones.arraySize = 1;
                SerializedProperty zone = zones.GetArrayElementAtIndex(0);
                zone.FindPropertyRelative("name").stringValue = "Quicksand";
                zone.FindPropertyRelative("normalizedPosition").floatValue = 0.3f;
                zone.FindPropertyRelative("normalizedHalfLength").floatValue = 0.02f;
                zone.FindPropertyRelative("width").floatValue = 5f;
                zone.FindPropertyRelative("material").objectReferenceValue = material;
                serializedTheme.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject serializedTrack = new SerializedObject(trackDef);
                serializedTrack.FindProperty("theme").objectReferenceValue = theme;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
                track.InitializeTrack(trackDef);

                Transform patch = trackGo.transform.Find("Track Theme/Track Effects/Quicksand");
                Assert.That(patch, Is.Not.Null);
                Assert.That(patch.localScale.x, Is.EqualTo(5f).Within(0.01f));
                Assert.That(
                    patch.localScale.z,
                    Is.EqualTo(0.04f * splineContainer.Spline.GetLength()).Within(0.01f));
                Assert.That(patch.GetComponent<Collider>(), Is.Null);

                trackGo.transform.position = new Vector3(4f, 0f, 3f);
                trackGo.transform.localScale = Vector3.one * 0.75f;
                track.RefreshThemeEffects();

                Transform refreshedPatch = trackGo.transform.Find("Track Theme/Track Effects/Quicksand");
                Assert.That(refreshedPatch, Is.Not.Null);
                Assert.That(refreshedPatch, Is.Not.SameAs(patch));
                Assert.That(refreshedPatch.gameObject.activeInHierarchy, Is.True);
                Assert.That(refreshedPatch.GetComponent<Renderer>().enabled, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(theme);
            }
        }

        [Test, Category("Biome")]
        public void InitializeTrack_RoadEdgeHasStrongValueContrastFromRoad()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Tracks/TrackViewComponent.prefab");
            Assert.That(prefab, Is.Not.Null);

            GameObject trackGo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            TrackThemeDefinition theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();
            try
            {
                Assert.That(trackGo, Is.Not.Null);
                TrackViewComponent track = trackGo.GetComponent<TrackViewComponent>();
                SeedOpenSpline(trackDef);

                Material roadMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/GearEngine/Art/Materials/TrackThemes/M_ThemeMountainRoad.mat");
                Assert.That(roadMaterial, Is.Not.Null);
                SerializedObject serializedTheme = new SerializedObject(theme);
                serializedTheme.FindProperty("roadMaterial").objectReferenceValue = roadMaterial;
                serializedTheme.FindProperty("useDefaultProps").boolValue = false;
                serializedTheme.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject serializedTrack = new SerializedObject(trackDef);
                serializedTrack.FindProperty("theme").objectReferenceValue = theme;
                serializedTrack.ApplyModifiedPropertiesWithoutUndo();
                track.InitializeTrack(trackDef);

                Renderer road = trackGo.transform.Find("Track/Path").GetComponent<Renderer>();
                Renderer roadEdge = trackGo.transform.Find("Track/RoadEdge").GetComponent<Renderer>();
                MaterialPropertyBlock edgeProperties = new MaterialPropertyBlock();
                roadEdge.GetPropertyBlock(edgeProperties);
                Color.RGBToHSV(road.sharedMaterial.GetColor("_BaseColor"), out _, out _, out float roadValue);
                Color.RGBToHSV(edgeProperties.GetColor("_BaseColor"), out _, out _, out float edgeValue);

                Assert.That(edgeValue - roadValue, Is.GreaterThanOrEqualTo(0.25f));
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
                Object.DestroyImmediate(trackDef);
                Object.DestroyImmediate(theme);
            }
        }

        private static void SeedOpenSpline(TrackDefinition trackDef)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
        }
    }
}
